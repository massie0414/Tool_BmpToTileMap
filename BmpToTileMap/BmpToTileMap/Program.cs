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
        const int color_max = 1;

        const int width = 384;  // TODO 動的にしたい
        const int height= 384;  // TODO 動的にしたい

        const int width_size = 48;
        const int height_size = 48;

        //const int file_size = 20789;  // 144x48
        //const int file_size = 30773;  // 160x64
        //const int file_size = 0x48035;  // 384x256
        const int file_end_address = 0x6C035;  // 384x384

        static void Main(string[] args)
        {
            int[] ints = new int[file_end_address + 1];
            List<byte> tileList = new List<byte>();
            List<byte> mapList = new List<byte>();


            // 1バイトずつ読み出し。
            using (BinaryReader w = new BinaryReader(File.OpenRead(@"convert.bmp")))
            {
                try
                {
                    for (int i = 0; i < file_end_address+1; i++)
                    {
                        ints[i] = w.ReadByte();
                    }
                }
                catch (EndOfStreamException)
                {
                    Console.Write("\n");
                }
            }

            int tile_max = 0;
            int[,] tiles = new int[256, 8];
            int[,] tile_map = new int[height_size, width_size];

            for (int color = 0; color < color_max; color++) // TODO このループはいらないけど…
            {
                int[,] b = new int[height, width];
                int[] gp = new int[width];

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        for (int l = 0; l < 3; l++)
                        {
                            int index = (file_end_address - i * width * 3 - j * 3 - l);
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

                    int[] diff_tile = new int[8];
                    int tile_map_index = 0;

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
                            // 終わりまで到達
                            Console.WriteLine(",");
                        }
                        else if (i % 8 == 7)
                        {
                            // 端まで到達
                            Console.WriteLine(",");
                            Console.Write("\t");
                        }
                        else
                        {
                            Console.Write(",");
                        }

                        diff_tile[i % 8] = gp[i];
                        if ( i % 8 == 7 )
                        {
                            // 比較データが揃った
                            bool isDiff = false;
                            for (int ii = 0; ii < 256; ii++)
                            {
                                if(ii == tile_max)
                                {
                                    // 未登録のタイル
                                    tile_map[j, tile_map_index] = ii;
                                    tile_max++;
                                    isDiff = true;
                                    for (int jj = 0; jj < 8; jj++)
                                    {
                                        tiles[ii, jj] = diff_tile[jj];
                                    }
                                    break;
                                }
                                for (int jj = 0; jj < 8; jj++)
                                {
                                    if (diff_tile[jj] != tiles[ii, jj])
                                    {
                                        break;
                                    }
                                    if(jj== 7)
                                    {
                                        // 最後までたどり着いたので同じタイル
                                        tile_map[j, tile_map_index] = ii;
                                        isDiff = true;
                                    }
                                }
                                if (isDiff)
                                {
                                    break;
                                }
                            }

                            tile_map_index++;
                        }

                        if (i == width - 1)
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
                Console.WriteLine("");

                // タイル
                Console.WriteLine("タイルデータ");
                for (int i = 0; i < 256; i++)
                {
                    Console.Write("tiles["+i+"]={");
                    for (int j = 0; j < 8; j++)
                    {
                        Console.Write("0x" + tiles[i, j].ToString("X2"));
                        Console.Write(",");

                        // ファイル出力用に書き込む
                        tileList.Add((byte)tiles[i, j]);
                    }
                    Console.WriteLine("};");
                }
                Console.WriteLine("");

                // マップ
                Console.WriteLine("マップデータ");
                for (int i = 0; i < height_size; i++)
                {
                    Console.Write("{");
                    for (int j = 0; j < width_size; j++)
                    {
                        Console.Write("0x" + tile_map[i, j].ToString("X2"));
                        Console.Write(",");

                        // ファイル出力用に書き込む
                        mapList.Add((byte)tile_map[i, j]);
                    }
                    Console.WriteLine("},");
                }
                Console.WriteLine("");


                Console.WriteLine("tile_max=");
                Console.WriteLine(tile_max);

            }

            // ファイル書き込み
            using (Stream stream = File.OpenWrite("tile.dat"))
            {
                // streamに書き込むためのBinaryWriterを作成
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    for (int i = 0; i < tileList.Count; i++)
                    {
                        writer.Write((byte)tileList[i]);
                    }
                }
            }

            using (Stream stream = File.OpenWrite("map.dat"))
            {
                // streamに書き込むためのBinaryWriterを作成
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    for (int i = 0; i < mapList.Count; i++)
                    {
                        writer.Write((byte)mapList[i]);
                    }
                }
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
