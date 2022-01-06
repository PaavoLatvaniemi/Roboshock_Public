using System;
//Matikkatyökaluja, geometrisia kaavoja.
namespace Assets.Scripts.PaerreMath
{
    class PaerreMath
    {
        //Tarvitaan ampumisessa. Lasketaan aseen rekyyliin perustuvasta ympyrästä random piste,
        //jonka perusteella lisätään rekyyliä aseella ampumiseen
        public static (double, double) getRandomVertexOnCircleShape(float circleRadius)
        {
            //Kaava random pisteen laskemiseen on 
            //X=(SQRT(r')*cos(th).
            //Tarvitaan Sqrt(r'), mutta randomilla välillä 0-r^2
            double circleRadiusRandom = getRandomDouble(0, circleRadius * circleRadius);
            //Myös tarvitaan kaksi random kulmaa cos ja sin välillä 0-2*PI. Kulma on th
            double randomTheta = getRandomDouble(0, (2 * Math.PI));
            double randomX = Math.Sqrt(circleRadiusRandom) * Math.Cos(randomTheta);
            double randomY = Math.Sqrt(circleRadiusRandom) * Math.Sin(randomTheta);
            return (randomX, randomY);
        }
        public static (double, double) getRandomVertexOnEllipseShape(float radiusXAxis, float radiusYAxis)
        {
            Random random = new Random();
            double circleRandomRadius = radiusXAxis * Math.Sqrt(random.NextDouble());
            //Value on NextDoublella 0.0-1.0
            double randomTheta = 2 * Math.PI * random.NextDouble();
            double randomX = circleRandomRadius * Math.Cos(randomTheta);
            double randomY = (radiusYAxis / radiusXAxis) * circleRandomRadius * Math.Sin(randomTheta);
            return (randomX, randomY);
        }

        private static double getRandomDouble(double min, double max)
        {
            Random random = new Random();
            //Antaa tulokseksi double väliä 0.0-1.0
            double randomValue = (float)random.NextDouble();
            //Mutta jos tämä vielä lasketaan näin, saadaan samaa tulosta, mutta rangella seuraavasti...
            randomValue = randomValue * (min - max) + min;
            return randomValue;
        }
    }
}
