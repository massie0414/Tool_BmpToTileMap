using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BmpToTileMap
{
    class Program
    {
        //  const int color_max = 1;
        const int color_max = 2;
        //  const int color_max = 3;
        //  const int color_max = 4;

        const int width = 160;  // TODO 動的にしたい
        const int height = 64;  // TODO 動的にしたい

        const int width_size = 6 * 8;  // 0~160
        const int height_size = 8;  // 0~8

        static void Main(string[] args)
        {
            int[] ints = new int[100000];

            // 1バイトずつ読み出し。
            using (BinaryReader w = new BinaryReader(File.OpenRead(@"convert.bmp")))
            {
                try
                {
                    for (int i = 0; i < 100000; i++)
                    {
                        ints[i] = w.ReadByte();
                    }
                }
                catch (EndOfStreamException)
                {
                    Console.Write("\n");
                }
            }

            for (int color = 0; color < color_max; color++)
            {
                int[,] b = new int[height, width];
                int[] gp = new int[width];

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        for (int l = 0; l < 3; l++)
                        {
                            //  int index = (20789 - i * width * 3 - j * 3 - l);    // TODO 20789がマジックナンバーすぎる 144x48
                            int index = (30773 - i * width * 3 - j * 3 - l);    // 160x64
                            b[i, j] += ((ints[index]));
                        }
                        switch (color_max)
                        {
                            case 1:
                                b[i, j] = Reversal2(b[i, j]);
                                break;
                            case 2:
                                b[i, j] = Reversal3(b[i, j], color);
                                break;
                            case 3:
                                b[i, j] = Reversal4(b[i, j], color);
                                break;
                            case 4:
                                b[i, j] = Reversal5(b[i, j], color);
                                break;
                        }
                    }
                }
                Console.WriteLine("{");
                for (int j = 0; j < height / 8; j++)
                {
                    Console.Write("\t");
                    for (int i = 0; i < width; i++)
                    {
                        gp[i] = (byte)(
                              b[0 + j * 8, width - 1 - i]
                            + b[1 + j * 8, width - 1 - i] * 0x02
                            + b[2 + j * 8, width - 1 - i] * 0x04
                            + b[3 + j * 8, width - 1 - i] * 0x08
                            + b[4 + j * 8, width - 1 - i] * 0x10
                            + b[5 + j * 8, width - 1 - i] * 0x20
                            + b[6 + j * 8, width - 1 - i] * 0x40
                            + b[7 + j * 8, width - 1 - i] * 0x80
                            );
                        Console.Write("0x" + gp[i].ToString("X2"));
                        if (i == width - 1)
                        {
                            Console.WriteLine(",");
                        }
                        else if (i % 8 == 7)
                        {
                            Console.WriteLine(",");
                            Console.Write("\t");
                        }
                        else
                        {
                            Console.Write(",");
                        }

                        if (i == width_size - 1)
                        {
                            break;
                        }
                    }
                    Console.WriteLine("");

                    if (j == height_size - 1)
                    {
                        break;
                    }
                }
                Console.WriteLine("},");
            }

            System.Threading.Thread.Sleep(100000);
        }

        /*
         * Fと0が逆なので、逆にしている
         */
        private static int Reversal(int b)
        {
            return (int)((b + 1) % 2);
        }

        /*
         * 5階調
         * BMPは白が765 黒が0
         * ポケコンは黒が1、白が0
         */
        private static int Reversal5(int b, int type)
        {
            switch (type)
            {
                case 0:
                    if (b < 616)
                    {
                        return 1;
                    }
                    break;
                case 1:
                    if (b < 308)
                    {
                        return 1;
                    }
                    break;
                case 2:
                    if (b < 462)
                    {
                        return 1;
                    }
                    break;
                case 3:
                    if (b < 154)
                    {
                        return 1;
                    }
                    break;
            }
            return 0;
        }

        /*
         * 4階調
         * BMPは白が765 黒が0
         * ポケコンは黒が1、白が0
         */
        private static int Reversal4(int b, int type)
        {
            switch (type)
            {
                case 0:
                    if (b < 576)
                    {
                        return 1;
                    }
                    break;
                case 1:
                    if (b < 384)
                    {
                        return 1;
                    }
                    break;
                case 2:
                    if (b < 192)
                    {
                        return 1;
                    }
                    break;
            }
            return 0;
        }

        /*
         * 3階調
         * BMPは白が765 黒が0
         * ポケコンは黒が1、白が0
         */
        private static int Reversal3(int b, int type)
        {
            switch (type)
            {
                case 0:
                    if (b < 100)
                    {
                        return 1;
                    }
                    break;
                case 1:
                    if (b < 700)
                    {
                        return 1;
                    }
                    break;
            }
            return 0;
        }

        /*
         * 2階調
         * BMPは白が765 黒が0
         * ポケコンは黒が1、白が0
         */
        private static int Reversal2(int b)
        {
            if (b < 100)
            //if (b < 384)
            //if (b < 600)
            {
                return 1;
            }
            return 0;
        }
    }
}
