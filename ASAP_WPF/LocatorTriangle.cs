using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Util;

namespace ASAP_WPF
{
    internal class LocatorTriangle
    {
        public PointF PointBBoxA { get; set; }
        public PointF PointBBoxB { get; set; }
        public PointF PointP{ get; set; }

        public PointF TransformedPointBBoxA { get; set; }
        public PointF TransformedPointBBoxB { get; set; }
        public PointF TransformedPointP { get; set; }
        public PointF TransformedPointPPlus { get; set; }
        public PointF TransformedPointPMinus { get; set; }

        public double AngleA { get;set; }
        public double AngleB { get; set; }
        public double AngleP { get; set; }

        public double LengthAB { get; set; }
        public double LengthAP { get; set; }
        public double LengthBP { get; set; }

        public LocatorTriangle(PointF first, PointF second, PointF third)
        {
            this.PointBBoxA = first;
            this.PointBBoxB = second;
            this.PointP = third;

            this.LengthAB = CalculateLengthBetweenPoint(PointBBoxA, PointBBoxB);
            this.LengthAP = CalculateLengthBetweenPoint(PointBBoxA, PointP);
            this.LengthBP = CalculateLengthBetweenPoint(PointBBoxB, PointP);

            this.AngleA = CalculateAngle(LengthAB,LengthAP,LengthBP);
            this.AngleB = CalculateAngle(LengthAB,LengthBP,LengthAP);
            this.AngleP = CalculateAngle(LengthAP,LengthBP,LengthAB);
        }

        private static double CalculateLengthBetweenPoint(PointF first, PointF second)
        {
            var length = double.NaN;
            var alpha = double.NaN;
            var beta = double.NaN;

            alpha = first.X - second.X;
            alpha = Math.Pow(alpha, 2);

            beta = first.Y - second.Y;
            beta = Math.Pow(beta, 2);

            length = alpha + beta;
            length = Math.Sqrt(length);
            return length;
        }

        private static double CalculateAngle(double firstSide, double secondSide, double opposingSide)
        {
            var alpha = Math.Pow(firstSide, 2);
            var beta = Math.Pow(secondSide, 2);
            var delta = Math.Pow(opposingSide, 2);

            var numerator = (alpha + beta - delta);
            var denominator = 2.0 * firstSide * secondSide;
            var fraction = numerator / denominator;

            var angle = Math.Acos(fraction);

            angle  *= (180 / Math.PI);

            return angle;
        }

        public (PointF,PointF) CalculateNewPPosition(PointF newA,PointF newB)
        {
            //http://paulbourke.net/geometry/circlesphere/
            //https://math.stackexchange.com/questions/543961/determine-third-point-of-triangle-when-two-points-and-all-sides-are-known
            //http://negativeprobability.blogspot.com/2011/11/affine-transformations-and-their.html
            //https://math.stackexchange.com/questions/1725790/calculate-third-point-of-triangle-from-two-points-and-angles
            //https://mathworld.wolfram.com/Circle-CircleIntersection.html
            //https://math.stackexchange.com/questions/256100/how-can-i-find-the-points-at-which-two-circles-intersect

            this.TransformedPointBBoxA = newA;
            this.TransformedPointBBoxB = newB;
            var ax = newA.X;
            var ay = newA.Y;
            var bx = newB.X;
            var by = newB.Y;
            var r1 = this.LengthAP;
            var r2 = this.LengthBP;
            var R = this.LengthAB;

            /*var alpha = Math.Pow(LengthAB, 2);
            var beta = Math.Pow(LengthAP, 2);
            var delta = Math.Pow(LengthBP, 2);

            var newPy = (alpha + beta - delta) / (2.0 * LengthAB);
            var newPx = Math.Sqrt(beta - Math.Pow(newPy, 2));*/



            //this.TransformedPointP = new PointF((float)newPx,(float)newPy);

            var plusPoint = PointF.Empty;
            var minusPoint = PointF.Empty;

            //(x,y)=1/2(x1+x2,y1+y2)+r21−r222R2(x2−x1,y2−y1)±122r21+r22R2−(r21−r22)2R4−1−−−−−−−−−−−−−−−−−−−−−√(y2−y1,x1−x2),

            //

            var basePoint = new PointF(ax + bx, ay + by).Multiply(0.5);
            var alpha = (Math.Pow(r1, 2) - Math.Pow(r2, 2)) /( 2 * Math.Pow(R, 2));
            var beta = new PointF(bx -ax, by-ay).Multiply(alpha);
            var delta =
                Math.Sqrt(2 * ( (Math.Pow(r1, 2) + Math.Pow(r2, 2))
                / (Math.Pow(R, 2))) - (Math.Pow(Math.Pow(r1, 2) - Math.Pow(r2, 2), 2) / (Math.Pow(R, 4)) -1));
            var gammaMinus = -(0.5) * delta;
            var gammaPlus  = +(0.5) * delta;

            var gammaMinusPoint = new PointF(by - ay, ax - bx).Multiply(gammaMinus);
            var gammaPlusPoint = new PointF(by - ay, ax - bx).Multiply(gammaPlus);

            this.TransformedPointPMinus = basePoint.Add(beta).Add(gammaMinusPoint);
            this.TransformedPointPPlus = basePoint.Add(beta).Add(gammaPlusPoint);


            return (TransformedPointPMinus, TransformedPointPPlus);
        }

        public void CalculateNewPPositionReverseAffine(Mat transformationMat)
        {
            var inverseTransformationMat = new Mat();
            CvInvoke.InvertAffineTransform(transformationMat, inverseTransformationMat);
            var ogPoints = new VectorOfPointF();
            var newPoints = new VectorOfPointF();
            ogPoints.Push(new []{this.PointBBoxA,this.PointBBoxB,this.PointP});
            CvInvoke.Transform(ogPoints,newPoints,inverseTransformationMat);
            var newPointsArray = newPoints.ToArray();

            this.TransformedPointBBoxA = newPointsArray[0];
            this.TransformedPointBBoxB = newPointsArray[1];
            this.TransformedPointP = newPointsArray[2];

        }
    }
}
