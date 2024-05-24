using System.Runtime.InteropServices.ComTypes;
using Unity.Collections;
using UnityEngine;



public class water : MonoBehaviour
{

    int [][]Height;
    byte[] BkGdImage;

    int WATERWID;
    int WATERHGT;
    int Hpage;
    int imgSize;
    float nextDropTime;
    Texture2D objTex;
    Texture2D objNorms;

    public int density = 10;
    public int dropradius = 10;
    public int dropheight = 10;
    public float dropTime = 1;

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

        Color[] TexPix = objTex.GetPixels(0);

        WATERWID = objTex.width;
        WATERHGT = objTex.height;
        Height[0] = new int[WATERWID * WATERHGT];
        Height[1] = new int[WATERWID * WATERHGT];

        imgSize = WATERWID * WATERHGT * 4;
        BkGdImage = new byte[imgSize * 3];

        Color32 norm = new Color32();

        norm.r = 0;
        norm.g = 0;
        norm.b = 255;


        for (int y = 0; y < WATERHGT; y++)
        {
            for (int x = 0; x < WATERWID; x++)
            {
                int ofs = imgSize + (y * WATERWID + x) * 4;

                Color32 pix = objTex.GetPixel(x, y);

                BkGdImage[ofs + 0] = pix.r;
                BkGdImage[ofs + 1] = pix.g;
                BkGdImage[ofs + 2] = pix.b;
                BkGdImage[ofs + 3] = pix.a;


                objNorms.SetPixel(x, y, norm);

            }
        }

        objNorms.Apply();

    }

    // Update is called once per frame
    void Update()
    {
        CalcWater(Hpage ^ 1, density);
        DrawWaterNoLight(Hpage);

        if(Time.fixedTime > nextDropTime)
        {
            int r = dropradius;
            int x = Random.Range(r, WATERWID - r);
            int y = Random.Range(r, WATERHGT - r);

            SineBlob(x, y, r, dropheight, Hpage);

            nextDropTime = Time.fixedTime + dropTime;
        }


        Hpage ^= 1;

    }




    void DrawWaterNoLight(int page)
    {

        int dx, dy;
        int x, y;
        Color32 npix = new Color32();
        Color32 norm = new Color32();
        byte r, g, b;

        int offset = WATERWID + 1;

        int[] ptr = Height[page];

        Color32[] TexPix = objTex.GetPixels32(0);
        Color32[] NormPix = objNorms.GetPixels32(0);

        Vector3 v1 = new Vector3(1, 0, 0);
        Vector3 v2 = new Vector3(0, 0, 1);
        Vector3 n;


        for (y = (WATERHGT - 1) * WATERWID; offset < y; offset += 2)
        {
            for (x = offset + WATERWID - 2; offset < x; offset++)
            {
                int tx = offset % WATERWID;
                int ty = offset / WATERWID;


                dx = ptr[offset] - ptr[offset + 1];
                dy = ptr[offset] - ptr[offset + WATERWID];

                v1.y = dx;
                v2.y = dy;

                n = Vector3.Cross(v1, v2);
                n.Normalize();

                int pofs = offset + WATERWID * (dy >> 3) + (dx >> 3);
                r = BkGdImage[imgSize + pofs * 4 + 0];
                g = BkGdImage[imgSize + pofs * 4 + 1];
                b = BkGdImage[imgSize + pofs * 4 + 2];
                
                /* If anyone knows a better/faster way to do this, please tell me... */
                npix.r = (byte)((r < 0) ? 0 : (r > 255) ? 255 : r);
                npix.g = (byte)((g < 0) ? 0 : (g > 255) ? 255 : g);
                npix.b = (byte)((b < 0) ? 0 : (b > 255) ? 255 : b);
                npix.a = 255;

                norm.r = (byte)((n.x + 1.0) * 127);
                norm.g = (byte)((n.y + 1.0) * 127);
                norm.b = (byte)((n.z + 1.0) * 127);

                
                TexPix[offset].r = npix.r;
                TexPix[offset].g = npix.g;
                TexPix[offset].b = npix.b;
                TexPix[offset].a = 255;

                //objTex.SetPixel(tx, ty, npix);

                NormPix[offset].a = norm.b;
                NormPix[offset].r = norm.r;
                NormPix[offset].g = norm.r;
                NormPix[offset].b = norm.r;
                

                //objNorms.SetPixel(tx, ty, norm);

                offset++;
                dx = ptr[offset] - ptr[offset + 1];
                dy = ptr[offset] - ptr[offset + WATERWID];

                v1.y = dx;
                v2.y = dy;

                n = Vector3.Cross(v1, v2);
                n.Normalize();

                pofs = offset + WATERWID * (dy >> 3) + (dx >> 3);
                r = BkGdImage[imgSize + pofs * 4 + 0];
                g = BkGdImage[imgSize + pofs * 4 + 1];
                b = BkGdImage[imgSize + pofs * 4 + 2];

                npix.r = (byte)((r < 0) ? 0 : (r > 255) ? 255 : r);
                npix.g = (byte)((g < 0) ? 0 : (g > 255) ? 255 : g);
                npix.b = (byte)((b < 0) ? 0 : (b > 255) ? 255 : b);
                npix.a = 255;

                norm.r = (byte)((n.x + 1.0) * 127);
                norm.g = (byte)((n.y + 1.0) * 127);
                norm.b = (byte)((n.z + 1.0) * 127);

                
                TexPix[offset].r = npix.r;
                TexPix[offset].g = npix.g;
                TexPix[offset].b = npix.b;
                TexPix[offset].a = 255;
                //objTex.SetPixel(tx + 1, ty, npix);

                NormPix[offset].a = norm.b;
                NormPix[offset].r = norm.r;
                NormPix[offset].g = norm.r;
                NormPix[offset].b = norm.r;

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
        if (x < 0) x = 1 + radius + Random.Range(0, (WATERWID - 2 * radius - 1));
        if (y < 0) y = 1 + radius + Random.Range(0, (WATERHGT - 2 * radius - 1));

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
        int square, dist;
        int radsquare = radius * radius;
        float length = (1.0f / (float)radius) * (1.0f / (float)radius);

        if (x < 0) x = 1 + radius + Random.Range(0, (WATERWID - 2 * radius - 1));
        if (y < 0) y = 1 + radius + Random.Range(0, (WATERHGT - 2 * radius - 1));


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
                    dist = (int)Mathf.Sqrt(square * length);
                    Height[page][WATERWID * (cy + y) + cx + x] += (int)((Mathf.Cos(dist) + 1) * (height));
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
