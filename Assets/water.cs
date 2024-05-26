using System;
using System.Runtime.InteropServices.ComTypes;
using Unity.Collections;
using Unity.Rendering.HybridV2;
using UnityEngine;



public class water : MonoBehaviour
{

    int [][]Height;
    uint[] BkGdImage;

    int WATERWID;
    int WATERHGT;
    int Hpage;
    int imgSize;
    float nextDropTime;
    Texture2D objTex;
    Texture2D objNorms;

    public int density = 8;
    public int dropradius = 32;
    public int dropheight = 64;
    public float dropTime = 0.5f;
    public int mode = 0;
    public Vector3 lightPos = new Vector3(256,256,256);

    // Start is called before the first frame update
    void Start()
    {

        Hpage = 0;
        Height = new int[2][];

        nextDropTime = 0;

        // If the object you want is the one this script is attached to:
        Renderer renderer = GetComponent<Renderer>();

        // This creates a special material just for this one object. If you change this material it will only affect this object:
        objTex = renderer.material.mainTexture as Texture2D;
        objNorms = renderer.material.GetTexture("_BumpMap") as Texture2D;

        WATERWID = objTex.width;
        WATERHGT = objTex.height;
        imgSize = WATERWID * WATERHGT;

        Height[0] = new int[imgSize];
        Height[1] = new int[imgSize];
        BkGdImage = new uint[imgSize * 3];

        // copy texture to image buffer
        Color[] TexPix = objTex.GetPixels(0);
        for (int y = 0; y < WATERHGT; y++)
        {
            for (int x = 0; x < WATERWID; x++)
            {
                int ofs = imgSize + (y * WATERWID + x);

                Color32 pix = objTex.GetPixel(x, y);

                BkGdImage[ofs + 0] = 0xFF000000 | (uint)((pix.r) | (pix.g << 8) | (pix.b << 16));
            }
        }

        // initialize normalmap
        if (objNorms)
        {
            Color32 norm = new Color32(0, 255, 0, 0);

            for (int y = 0; y < WATERHGT; y++)
            {
                for (int x = 0; x < WATERWID; x++)
                {
                    objNorms.SetPixel(x, y, norm);
                }
            }
            objNorms.Apply();
        }
        else
            mode = 1;

    }

    // Update is called once per frame
    void Update()
    {
        CalcWater(Hpage ^ 1, density);

        if(mode == 0)
            DrawWaterNoLight(Hpage);
        else
            DrawWaterLight(Hpage, lightPos);

        if (Time.fixedTime > nextDropTime)
        {
            int r = dropradius;
            int x = UnityEngine.Random.Range(r, WATERWID - r);
            int y = UnityEngine.Random.Range(r, WATERHGT - r);

            SineBlob(x, y, r, dropheight, Hpage);

            nextDropTime = Time.fixedTime + dropTime;
        }


        Hpage ^= 1;

    }

    float InvSqrt(double x)
    {
        double  xhalf = 0.5 * x;
        long i = BitConverter.DoubleToInt64Bits(x);
        i = 0x5f3759df - (i >> 1);
        x = BitConverter.Int64BitsToDouble(i);
        x = x * (1.5f - xhalf * x * x);
        return (float)x;
    }

    void DrawWaterLight(int page, Vector3 lightPos)
    {
        int dx, dy;
        int x, y;
        uint pix;
        int offset = WATERWID + 1;

        int[] ptr = Height[page];

        Color32[] TexPix = objTex.GetPixels32(0);
        float normX;
        float normY;
        float normZ;
        float sql;
        float il;
        float NdL;

        Vector3 vecToLight = new Vector3();

        for (y = (WATERHGT - 1) * WATERWID; offset < y; offset += 2)
        {
            for (x = offset + WATERWID - 2; offset < x; offset++)
            {
                int tx = offset % WATERWID;
                int ty = offset / WATERWID;

                dx = ptr[offset] - ptr[offset + 1];
                dy = ptr[offset] - ptr[offset + WATERWID];

                pix = BkGdImage[imgSize + offset + WATERWID * (dy >> 3) + (dx >> 3)];

                normX = dx;
                normY = 100;
                normZ = dy;
                sql = normX * normX + normY * normY + normZ * normZ;
                il = 1.0f / Mathf.Sqrt(sql);
                normX *= il;
                normY *= il;
                normZ *= il;

                vecToLight.x = lightPos.x - tx;
                vecToLight.y = lightPos.y;
                vecToLight.z = lightPos.z - ty;
                vecToLight.Normalize();

                NdL = Mathf.Max(0.1f, vecToLight.x * normX + vecToLight.y * normY + vecToLight.z * normZ);

                TexPix[offset].r = (byte)(Math.Min(255,(pix & 0xFF) * NdL));
                TexPix[offset].g = (byte)(Math.Min(255, ((pix >> 8) & 0xFF) * NdL));
                TexPix[offset].b = (byte)(Math.Min(255, ((pix >> 16) & 0xFF) * NdL));
                TexPix[offset].a = 255;
                //objTex.SetPixel(tx, ty, npix);


                //objNorms.SetPixel(tx, ty, norm);

                offset++;
                tx++;

                dx = ptr[offset] - ptr[offset + 1];
                dy = ptr[offset] - ptr[offset + WATERWID];

                pix = BkGdImage[imgSize + offset + WATERWID * (dy >> 3) + (dx >> 3)];

                normX = dx;
                normY = 100;
                normZ = dy;
                sql = normX * normX + normY * normY + normZ * normZ;
                il = 1.0f / Mathf.Sqrt(sql);
                normX *= il;
                normY *= il;
                normZ *= il;

                vecToLight.x = lightPos.x - tx;
                vecToLight.y = lightPos.y;
                vecToLight.z = lightPos.z - ty;
                vecToLight.Normalize();

                NdL = Mathf.Max(0.1f, vecToLight.x * normX + vecToLight.y * normY + vecToLight.z * normZ);

                TexPix[offset].r = (byte)(Math.Min(255, (pix & 0xFF) * NdL));
                TexPix[offset].g = (byte)(Math.Min(255, ((pix >> 8) & 0xFF) * NdL));
                TexPix[offset].b = (byte)(Math.Min(255, ((pix >> 16) & 0xFF) * NdL));
                TexPix[offset].a = 255;

                //objNorms.SetPixel(tx + 1, ty, norm);

            }
        }

        objTex.SetPixels32(TexPix, 0);
        objTex.Apply();
    }

    void DrawWaterNoLight(int page)
    {
        int dx, dy;
        int x, y;
        uint pix;
        int offset = WATERWID + 1;

        int[] ptr = Height[page];

        Color32[] TexPix = objTex.GetPixels32(0);
        Color32[] NormPix = objNorms.GetPixels32(0);

        float normX;
        float normY = 100;
        float normZ;
        float sql;
        float il;

        for (y = (WATERHGT - 1) * WATERWID; offset < y; offset += 2)
        {
            for (x = offset + WATERWID - 2; offset < x; offset++)
            {
                //int tx = offset % WATERWID;
                //int ty = offset / WATERWID;

                dx = ptr[offset] - ptr[offset + 1];
                dy = ptr[offset] - ptr[offset + WATERWID];

                pix = BkGdImage[imgSize + offset + WATERWID * (dy >> 3) + (dx >> 3)];

                TexPix[offset].r = (byte)(pix & 0xFF);
                TexPix[offset].g = (byte)((pix >> 8) & 0xFF);
                TexPix[offset].b = (byte)((pix >> 16) & 0xFF);
                TexPix[offset].a = 255;
                //objTex.SetPixel(tx, ty, npix);

                normX = dx;
                normZ = dy;
                sql = normX * normX + 10000 + normZ * normZ;
                il = 1.0f / Mathf.Sqrt(sql);
                normX *= il;
                normZ *= il;

                NormPix[offset].a = (byte)((normZ + 1.0) * 127);
                NormPix[offset].r = (byte)((normX + 1.0) * 127);
                NormPix[offset].g = NormPix[offset].r;
                NormPix[offset].b = NormPix[offset].r;


                //objNorms.SetPixel(tx, ty, norm);

                offset++;

                dx = ptr[offset] - ptr[offset + 1];
                dy = ptr[offset] - ptr[offset + WATERWID];

                pix = BkGdImage[imgSize + offset + WATERWID * (dy >> 3) + (dx >> 3)];

                TexPix[offset].r = (byte)(pix & 0xFF);
                TexPix[offset].g = (byte)((pix >> 8) & 0xFF);
                TexPix[offset].b = (byte)((pix >> 16) & 0xFF);
                TexPix[offset].a = 255;
                //objTex.SetPixel(tx, ty, npix);

                normX = dx;
                normZ = dy;
                sql = normX * normX + 10000 + normZ * normZ;
                il = 1.0f / Mathf.Sqrt(sql);
                normX *= il;
                normZ *= il;

                NormPix[offset].a = (byte)((normZ + 1.0) * 127);
                NormPix[offset].r = (byte)((normX + 1.0) * 127);
                NormPix[offset].g = NormPix[offset].r;
                NormPix[offset].b = NormPix[offset].r;

                //objNorms.SetPixel(tx + 1, ty, norm);

            }
        }

        objTex.SetPixels32(TexPix, 0);
        objTex.Apply();

        objNorms.SetPixels32(NormPix, 0);
        objNorms.Apply();
    }


    void HeightBlob(int x, int y, int radius, int height, int page)
    {
        int rquad;
        int cx, cy, cyq;
        int left, top, right, bottom;


        rquad = radius * radius;

        /* Make a randomly-placed blob... */
        if (x < 0) x = UnityEngine.Random.Range(1 + radius, (WATERWID - 2 * radius - 1));
        if (y < 0) y = UnityEngine.Random.Range(1 + radius, (WATERHGT - 2 * radius - 1));

        left = -radius; right = radius;
        top = -radius; bottom = radius;

        /* Perform edge clipping... */
        if (x - radius < 1) left -= (x - radius - 1);
        if (y - radius < 1) top -= (y - radius - 1);
        if (x + radius > WATERWID - 1) right -= (x + radius - WATERWID + 1);
        if (y + radius > WATERHGT - 1) bottom -= (y + radius - WATERHGT + 1);


        for (cy = top; cy < bottom; cy++)
        {
            cyq = cy * cy;
            for (cx = left; cx < right; cx++)
            {
                if (cx * cx + cyq < rquad)
                    Height[page][WATERWID * (cy + y) + (cx + x)] += height;
            }
        }
    }

    void SineBlob(int x, int y, int radius, int height, int page)
    {
        int cx, cy;
        int left, top, right, bottom;
        float square, dist;
        float radsquare = radius * radius;
        float length = 1.0f / radsquare;

        if (x < 0) x = UnityEngine.Random.Range(1 + radius, (WATERWID - 2 * radius - 1));
        if (y < 0) y = UnityEngine.Random.Range(1 + radius, (WATERHGT - 2 * radius - 1));


        radsquare = (radius * radius);

        left = -radius; right = radius;
        top = -radius; bottom = radius;


        /* Perform edge clipping... */
        if (x - radius < 1) left -= (x - radius - 1);
        if (y - radius < 1) top -= (y - radius - 1);
        if (x + radius > WATERWID - 1) right -= (x + radius - WATERWID + 1);
        if (y + radius > WATERHGT - 1) bottom -= (y + radius - WATERHGT + 1);

        for (cy = top; cy < bottom; cy++)
        {
            for (cx = left; cx < right; cx++)
            {
                square = cy * cy + cx * cx;
                if (square < radsquare)
                {
                    dist = Mathf.Sqrt(square * length);
                    Height[page][WATERWID * (cy + y) + cx + x] += (int)((Mathf.Cos(dist * Mathf.PI) + 1) * (height));
                }
            }
        }
    }

    void CalcWater(int npage, int density)
    {
        int newh;
        int count = WATERWID + 1;

        int []newptr = Height[npage];
        int []oldptr = Height[npage ^ 1];

        int x, y;

        /* Sorry, this function might not be as readable as I'd like, because
           I optimized it somewhat.  (enough to make me feel satisfied with it)
         */
        for (y = (WATERHGT - 1) * WATERWID; count < y; count += 2)
        {
            for (x = count + WATERWID - 2; count < x; count++)
            {
                /* This does the eight-pixel method.  It looks much better. */

                newh = ((oldptr[count + WATERWID]
                                + oldptr[count - WATERWID]
                                + oldptr[count + 1]
                                + oldptr[count - 1]
                                + oldptr[count - WATERWID - 1]
                                + oldptr[count - WATERWID + 1]
                                + oldptr[count + WATERWID - 1]
                                + oldptr[count + WATERWID + 1]
                                 ) >> 2)
                                - newptr[count];


                newptr[count] = newh - (newh >> density);
            }
        }
    }
}
