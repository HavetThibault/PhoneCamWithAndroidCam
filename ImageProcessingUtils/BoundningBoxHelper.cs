using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils
{
    internal static class BoundingBoxHelper
    {

        /// <param name="byteArray">Must be unstrided and thresholded 0 or 255, 0 is background and 255 are items</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Rectangle CalculateBoundingBox(byte[] byteArray, int width, int height)
        {
            // On calcule M:
            // M = (Jxx Jxy)
            //     (Jyx Jyy)

            int Jxx = 0, Jxy = 0, Jyy = 0;
            int length = width * height;
            int x, y;
            for (int i = 0; i < length; i++)
            {
                if (byteArray[i] == 255)
                {
                    x = i % width + 1;
                    y = i / width + 1;
                    Jxx += x * x;
                    Jxy += x * y;
                    Jyy += y * y;
                }
            }

            // Il faut calculer M' = P^(-1)*M*P
            // Il faut calculer les valeurs propres de M, pour ensuite
            // trouver les valeurs propres de M qui donnent P (en les concaténant) :

            // Valeurs propres :
            // Il faut trouver dtm(A - lambda*I) = 0
            //      (Jxx - lambda) * (Jyy - lambda) - (Jyx * Jxy) = 0       OR Jxy = Jyx
            // <=>  lambda² - (Jxx + Jyy) * lambda + Jxx * Jyy - Jxy² = 0
            // => Trouver les valeurs propres = les endroits ou le polynôme s'annule
            long delta = (Jxx + Jyy) * (Jxx + Jyy) - 4 * (Jxx * Jyy - Jxy * Jxy);
            double valeurPropre1 = 0.5 * Jxx * Jyy + Math.Sqrt(delta);
            double ValeurPropre2 = 0.5 * Jxx * Jyy - Math.Sqrt(delta);

            return new Rectangle(0, 0, 0, 0);
        }
    }
}
