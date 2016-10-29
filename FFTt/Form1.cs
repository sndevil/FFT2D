using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Numerics;



namespace FFTt
{
    public partial class Form1 : Form
    {
        const float PI = 3.14159265f;
        Bitmap temp,temp2,temp3,first;

        public Form1()
        {
            InitializeComponent();
            first = new Bitmap((Image)pictureBox1.Image.Clone());

            temp = new Bitmap(first.Width, first.Height);
            temp2 = new Bitmap(first.Width, first.Height);
            temp3 = new Bitmap(first.Width, first.Height);
        }

        private void ProcessBtn_Click(object sender, EventArgs e)
        {
            Complex[][] bufR = new Complex[first.Height][];
            Complex[][] bufRout = new Complex[first.Height][];
            Complex[][] bufG = new Complex[first.Height][];
            Complex[][] bufGout = new Complex[first.Height][];
            Complex[][] bufB = new Complex[first.Height][];
            Complex[][] bufBout = new Complex[first.Height][];
            for (int i = 0; i < first.Height; i++)
            {
                bufR[i] = new Complex[first.Width];
                bufRout[i] = new Complex[first.Width];
                bufG[i] = new Complex[first.Width];
                bufGout[i] = new Complex[first.Width];
                bufB[i] = new Complex[first.Width];
                bufBout[i] = new Complex[first.Width];
                for (int j = 0; j < first.Width; j ++)
                {
                    var color = first.GetPixel(j, i);
                    var tempR = (int)color.R;
                    var tempG = (int)color.G;
                    var tempB = (int)color.B;
                    bufR[i][j] = new Complex(tempR,0);
                    bufRout[i][j] = new Complex(0, 0);
                    bufG[i][j] = new Complex(tempG, 0);
                    bufGout[i][j] = new Complex(0, 0);
                    bufB[i][j] = new Complex(tempB, 0);
                    bufBout[i][j] = new Complex(0, 0);
                }
            }
            FFT2D(bufR,bufRout);
            FFT2D(bufG,bufGout);
            FFT2D(bufB,bufBout);

            int R,G,B;
            for (int i = 0; i < first.Width; i++)
                for (int j = 0; j < first.Height; j++)
                {
                    /*var R = (int)bufR[j][i].Magnitude;
                    if (R > maxR)
                        maxR = R;
                    else if (R < minR)
                        minR = R;*/
                    R = (int)(bufRout[j][i].Magnitude*5);//64);
                    G = (int)(bufGout[j][i].Magnitude*5);//64);
                    B = (int)(bufBout[j][i].Magnitude*5);//64);
                    if (R > 255)
                    { R = 255; }
                    if (G > 255)
                    { G = 255; }
                    if (B > 255)
                    { B = 255; }

                    var ColR = Color.FromArgb(R,G,B);
                    temp.SetPixel(i, j, ColR);
                    var ColG = Color.FromArgb(0,G,0);
                    temp2.SetPixel(i, j, ColG);
                    var ColB = Color.FromArgb(0,0,B);
                    temp3.SetPixel(i, j, ColB);
                }

            pictureBox1.Image = temp;
            temp.Save("E:\\out1.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            temp2.Save("E:\\out2.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            temp3.Save("E:\\out3.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

        }

        public static int BitReverse(int n, int bits)
        {
            int reversedN = n;
            int count = bits - 1;

            n >>= 1;
            while (n > 0)
            {
                reversedN = (reversedN << 1) | (n & 1);
                count--;
                n >>= 1;
            }

            return ((reversedN << count) & ((1 << bits) - 1));
        }

        public static void FFT(Complex[] buffer)
        {

            int bits = (int)Math.Log(buffer.Length, 2);
            for (int j = 1; j < buffer.Length / 2; j++)
            {

                int swapPos = BitReverse(j, bits);
                var temp = buffer[j];
                buffer[j] = buffer[swapPos];
                buffer[swapPos] = temp;
            }

            for (int N = 2; N <= buffer.Length; N <<= 1)
            {
                for (int i = 0; i < buffer.Length; i += N)
                {
                    for (int k = 0; k < N / 2; k++)
                    {

                        int evenIndex = i + k;
                        int oddIndex = i + k + (N / 2);
                        var even = buffer[evenIndex];
                        var odd = buffer[oddIndex];

                        double term = -2 * Math.PI * k / (double)N;
                        Complex exp = new Complex(Math.Cos(term), Math.Sin(term)) * odd;

                        buffer[evenIndex] = even + exp;
                        buffer[oddIndex] = even - exp;

                    }
                }
            }
        }

        public static void FFT2D(Complex[][] buffer,Complex[][] outbuffer)
        {
            Complex[] tempbuffer;
            for (int i = 0; i < buffer.Length-32; i += 32)
            {
                for (int j = 0; j < buffer[0].Length-32; j += 32)
                {
                    tempbuffer = new Complex[64];
                    for (int count = 0; count < 64; count++)
                    {
                        for (int k = j; k < j + 64; k++)
                            tempbuffer[k - j] = buffer[i+count][k];
                        FFT(tempbuffer);
                        for (int k = j; k < j + 64; k++)
                        {
                            var mag = 1 - Math.Abs((double)(k - j - 32) / 32);
                            outbuffer[i + count][k] += (tempbuffer[k - j] / 64) * mag;
                        }
                    }
                    for (int count = 0; count < 64; count++)
                    {
                        for (int k = i; k < i + 64; k++)
                            tempbuffer[k - i] = buffer[k][j+count];
                        FFT(tempbuffer);
                        for (int k = i; k < i + 64; k++)
                        {
                            var mag = 1 - Math.Abs((double)(k - i - 32) / 32);
                            outbuffer[k][j + count] += (tempbuffer[k - i] / 64) * mag;
                        }
                    }
                    

                }
            }
        }

    }
}



