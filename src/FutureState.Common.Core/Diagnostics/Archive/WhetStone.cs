#region

using System;
using System.Diagnostics;

#endregion

namespace FutureState.Diagnostics
{
    // originally taken from source forge http://sourceforge.net/projects/netlinawhetcpu/

    /// <summary>
    /// A quick and dirty processor speed calculator yielding a whetstone benchmark.
    /// </summary>
    public class Whetstone
    {
        private const double t = 0.499975;

        private const double t1 = 0.50025;

        private const double t2 = 2.0;

        private readonly Stopwatch _sw = new Stopwatch();

        private readonly double[] _z = new double[1];

        private readonly double[] e1 = new double[4];

        private int _cycleNo;

        private int _i;

        private int _j;

        private int _k;

        private int _l;

        private int _n1;

        private double _x;

        private double _x1;

        private double _x2;

        private double _x3;

        private double _x4;

        private double _y;

        private int n10;

        private int n11;

        private int n2;

        private int n3;

        private int n4;

        private int n6;

        private int n7;

        private int n8;

        private int n9;

        public long BeginTime { get; set; }

        public long EndTime { get; set; }

        public int Iterations { get; set; }

        public int NumberOfCycles { get; set; }

        public double StartCalc()
        {
            /* set values of module weights */
            _n1 = 0*Iterations;
            n2 = 12*Iterations;
            n3 = 14*Iterations;
            n4 = 345*Iterations;
            n6 = 210*Iterations;
            n7 = 32*Iterations;
            n8 = 899*Iterations;
            n9 = 616*Iterations;
            n10 = 0*Iterations;
            n11 = 93*Iterations;

            BeginTime = 0;
            _sw.Reset();
            _sw.Start();

            for (_cycleNo = 1; _cycleNo <= NumberOfCycles; _cycleNo++)
            {
                /* MODULE 1: simple identifiers */
                _x1 = 1.0;
                _x2 = _x3 = _x4 = -1.0;
                for (_i = 1; _i <= _n1; _i += 1)
                {
                    _x1 = (_x1 + _x2 + _x3 - _x4)*t;
                    _x2 = (_x1 + _x2 - _x3 + _x4)*t; // correction: x2 = ( x1 + x2 - x3 - x4 ) * t;
                    _x3 = (_x1 - _x2 + _x3 + _x4)*t; // correction: x3 = ( x1 - x2 + x3 + x4 ) * t;
                    _x4 = (-_x1 + _x2 + _x3 + _x4)*t;
                }

                /* MODULE 2: array elements */
                e1[0] = 1.0;
                e1[1] = e1[2] = e1[3] = -1.0;
                for (_i = 1; _i <= n2; _i += 1)
                {
                    e1[0] = (e1[0] + e1[1] + e1[2] - e1[3])*t;
                    e1[1] = (e1[0] + e1[1] - e1[2] + e1[3])*t;
                    e1[2] = (e1[0] - e1[1] + e1[2] + e1[3])*t;
                    e1[3] = (-e1[0] + e1[1] + e1[2] + e1[3])*t;
                }

                /* MODULE 3: array as parameter */
                for (_i = 1; _i <= n3; _i += 1)
                {
                    PA(e1);
                }

                /* MODULE 4: conditional jumps */
                _j = 1;
                for (_i = 1; _i <= n4; _i += 1)
                {
                    if (_j == 1)
                    {
                        _j = 2;
                    }
                    else
                    {
                        _j = 3;
                    }

                    if (_j > 2)
                    {
                        _j = 0;
                    }
                    else
                    {
                        _j = 1;
                    }

                    if (_j < 1)
                    {
                        _j = 1;
                    }
                    else
                    {
                        _j = 0;
                    }
                }

                /* MODULE 5: omitted */
                /* MODULE 6: integer arithmetic */
                _j = 1;
                _k = 2;
                _l = 3;
                for (_i = 1; _i <= n6; _i += 1)
                {
                    _j = _j*(_k - _j)*(_l - _k);
                    _k = _l*_k - (_l - _j)*_k;
                    _l = (_l - _k)*(_k + _j);
                    e1[_l - 2] = _j + _k + _l; /* C arrays are zero based */
                    e1[_k - 2] = _j*_k*_l;
                }

                /* MODULE 7: trig. functions */
                _x = _y = 0.5;
                for (_i = 1; _i <= n7; _i += 1)
                {
                    _x = t*Math.Atan(t2*Math.Sin(_x)*Math.Cos(_x)/(Math.Cos(_x + _y) + Math.Cos(_x - _y) - 1.0));
                    _y = t*Math.Atan(t2*Math.Sin(_y)*Math.Cos(_y)/(Math.Cos(_x + _y) + Math.Cos(_x - _y) - 1.0));
                }

                /* MODULE 8: procedure calls */
                _x = _y = _z[0] = 1.0;
                for (_i = 1; _i <= n8; _i += 1)
                {
                    P3(_x, _y, _z);
                }

                /* MODULE9: array references */
                _j = 0;
                _k = 1;
                _l = 2;
                e1[0] = 1.0;
                e1[1] = 2.0;
                e1[2] = 3.0;
                for (_i = 1; _i <= n9; _i++)
                {
                    P0();
                }

                /* MODULE10: integer arithmetic */
                _j = 2;
                _k = 3;
                for (_i = 1; _i <= n10; _i += 1)
                {
                    _j = _j + _k;
                    _k = _j + _k;
                    _j = _k - _j;
                    _k = _k - _j - _j;
                }

                /* MODULE11: standard functions */
                _x = 0.75;
                for (_i = 1; _i <= n11; _i += 1)
                {
                    _x = Math.Sqrt(Math.Exp(Math.Log(_x)/t1));
                }
            }

            _sw.Stop();
            EndTime = _sw.ElapsedMilliseconds;

            return EndTime - BeginTime;
        }

        private void P0()
        {
            e1[_j] = e1[_k];
            e1[_k] = e1[_l];
            e1[_l] = e1[_j];
        }

        private void P3(double x, double y, double[] z)
        {
            _x = t*(x + y);
            _y = t*(x + y);
            z[0] = (x + y)/t2;
        }

        private void PA(double[] e)
        {
            int j;
            j = 0;
            do
            {
                e[0] = (e[0] + e[1] + e[2] - e[3])*t;
                e[1] = (e[0] + e[1] - e[2] + e[3])*t;
                e[2] = (e[0] - e[1] + e[2] + e[3])*t;
                e[3] = (-e[0] + e[1] + e[2] + e[3])/t2;
                j += 1;
            } while (j < 6);
        }
    }
}