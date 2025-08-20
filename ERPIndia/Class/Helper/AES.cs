using System;

namespace ERPIndia.Class.Helper
{
    /// <summary>
    /// AES encryption class.
    /// </summary>
    public class AES
    {
        #region Variable Declaration

        private byte[] sbox = {
                                    0x63, 0x7c, 0x77, 0x7b, 0xf2, 0x6b, 0x6f, 0xc5, 0x30, 0x01, 0x67, 0x2b, 0xfe, 0xd7, 0xab, 0x76,
                                    0xca, 0x82, 0xc9, 0x7d, 0xfa, 0x59, 0x47, 0xf0, 0xad, 0xd4, 0xa2, 0xaf, 0x9c, 0xa4, 0x72, 0xc0,
                                    0xb7, 0xfd, 0x93, 0x26, 0x36, 0x3f, 0xf7, 0xcc, 0x34, 0xa5, 0xe5, 0xf1, 0x71, 0xd8, 0x31, 0x15,
                                    0x04, 0xc7, 0x23, 0xc3, 0x18, 0x96, 0x05, 0x9a, 0x07, 0x12, 0x80, 0xe2, 0xeb, 0x27, 0xb2, 0x75,
                                    0x09, 0x83, 0x2c, 0x1a, 0x1b, 0x6e, 0x5a, 0xa0, 0x52, 0x3b, 0xd6, 0xb3, 0x29, 0xe3, 0x2f, 0x84,
                                    0x53, 0xd1, 0x00, 0xed, 0x20, 0xfc, 0xb1, 0x5b, 0x6a, 0xcb, 0xbe, 0x39, 0x4a, 0x4c, 0x58, 0xcf,
                                    0xd0, 0xef, 0xaa, 0xfb, 0x43, 0x4d, 0x33, 0x85, 0x45, 0xf9, 0x02, 0x7f, 0x50, 0x3c, 0x9f, 0xa8,
                                    0x51, 0xa3, 0x40, 0x8f, 0x92, 0x9d, 0x38, 0xf5, 0xbc, 0xb6, 0xda, 0x21, 0x10, 0xff, 0xf3, 0xd2,
                                    0xcd, 0x0c, 0x13, 0xec, 0x5f, 0x97, 0x44, 0x17, 0xc4, 0xa7, 0x7e, 0x3d, 0x64, 0x5d, 0x19, 0x73,
                                    0x60, 0x81, 0x4f, 0xdc, 0x22, 0x2a, 0x90, 0x88, 0x46, 0xee, 0xb8, 0x14, 0xde, 0x5e, 0x0b, 0xdb,
                                    0xe0, 0x32, 0x3a, 0x0a, 0x49, 0x06, 0x24, 0x5c, 0xc2, 0xd3, 0xac, 0x62, 0x91, 0x95, 0xe4, 0x79,
                                    0xe7, 0xc8, 0x37, 0x6d, 0x8d, 0xd5, 0x4e, 0xa9, 0x6c, 0x56, 0xf4, 0xea, 0x65, 0x7a, 0xae, 0x08,
                                    0xba, 0x78, 0x25, 0x2e, 0x1c, 0xa6, 0xb4, 0xc6, 0xe8, 0xdd, 0x74, 0x1f, 0x4b, 0xbd, 0x8b, 0x8a,
                                    0x70, 0x3e, 0xb5, 0x66, 0x48, 0x03, 0xf6, 0x0e, 0x61, 0x35, 0x57, 0xb9, 0x86, 0xc1, 0x1d, 0x9e,
                                    0xe1, 0xf8, 0x98, 0x11, 0x69, 0xd9, 0x8e, 0x94, 0x9b, 0x1e, 0x87, 0xe9, 0xce, 0x55, 0x28, 0xdf,
                                    0x8c, 0xa1, 0x89, 0x0d, 0xbf, 0xe6, 0x42, 0x68, 0x41, 0x99, 0x2d, 0x0f, 0xb0, 0x54, 0xbb, 0x16
                              };

        //// Rcon is Round Constant used for the Key Expansion [1st col is 2^(r-1) in GF(2^8)] [§5.2]
        private byte[,] rcon = { { 0x00, 0x00, 0x00, 0x00 }, { 0x01, 0x00, 0x00, 0x00 }, { 0x02, 0x00, 0x00, 0x00 }, { 0x04, 0x00, 0x00, 0x00 }, { 0x08, 0x00, 0x00, 0x00 }, { 0x10, 0x00, 0x00, 0x00 }, { 0x20, 0x00, 0x00, 0x00 }, { 0x40, 0x00, 0x00, 0x00 }, { 0x80, 0x00, 0x00, 0x00 }, { 0x1b, 0x00, 0x00, 0x00 }, { 0x36, 0x00, 0x00, 0x00 } };

        private string b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!-=";

        #endregion

        #region Public Methods

        /// <summary>
        /// Encrypts string using AES.
        /// </summary>
        /// <param name="plaintext">The plaintext.</param>
        /// <param name="password">The password.</param>
        /// <param name="noOfBits">The no of bits.</param>
        /// <returns>Returns encrypted string.</returns>
        public string AESEncryptCtr(string plaintext, string password, int noOfBits)
        {
            int blockSize = 16;  //// block size fixed at 16 bytes / 128 bits (Nb=4) for AES
            ////int nBits = 128;     ////number of bits to be used in the key (128, 192, or 256)
            if (!(noOfBits == 128 || noOfBits == 192 || noOfBits == 256))
            {
                return string.Empty;  //// standard allows 128/192/256 bit keys
            }

            //// use AES itself to encrypt password to get cipher key (using plain password as source for key 
            //// expansion) - gives us well encrypted key
            int noOfBytes = noOfBits / 8;  //// no bytes in key
            byte[] passwordBytes = new byte[noOfBytes];

            for (int i = 0; (i < password.Length) && (i < passwordBytes.Length); i++)
            {
                char c = Convert.ToChar(password.Substring(i, 1).ToString());
                passwordBytes[i] = Convert.ToByte(c);
            }

            byte[] key = this.Cipher(passwordBytes, this.KeyExpansion(passwordBytes));  //// gives us 16-byte key
            //// initialise counter block (NIST SP800-38A §B.2): millisecond time-stamp for nonce in 1st 8 bytes,
            //// block counter in 2nd 8 bytes
            ////byte[] newkey = expandKey(key, 24);
            key = this.expandKey(key, noOfBytes);
            byte[] counterBlock = new byte[blockSize];

            ////1219239688140 timestamp 
            ////Int64 nonce = 1219239688140;  //// timestamp: milliseconds since 1-Jan-1970
            long nonce = this.GetTime();  //// timestamp: milliseconds since 1-Jan-1970
            int nonceSec = (int)Math.Floor((double)nonce / 1000);
            int nonceMs = (int)(nonce % 1000);

            //// encode nonce with seconds in 1st 4 bytes, and (repeated) ms part filling 2nd 4 bytes
            for (int i = 0; i < 4; i++)
            {
                counterBlock[i] = (byte)((nonceSec >> i * 8) & 0xff);
            }

            for (int i = 0; i < 4; i++)
            {
                counterBlock[i + 4] = (byte)(nonceMs & 0xff);
            }
            //// and convert it to a string to go on the front of the ciphertext

            string ctrTxt = string.Empty;

            for (int i = 0; i < 8; i++)
            {
                ctrTxt += (char)counterBlock[i];
            }

            //// generate key schedule - an expansion of the key into distinct Key Rounds for each round
            byte[][] keySchedule = this.KeyExpansion(key);

            int blockCount = (int)Math.Ceiling((double)plaintext.Length / (double)blockSize);
            string[] ciphertxt = new string[blockCount];  //// ciphertext as array of strings

            for (int b = 0; b < blockCount; b++)
            {
                //// set counter (block #) in last 8 bytes of counter block (leaving nonce in 1st 8 bytes)
                //// done in two stages for 32-bit ops: using two words allows us to go past 2^32 blocks (68GB)
                for (int c = 0; c < 4; c++)
                {
                    counterBlock[15 - c] = (byte)((b >> c * 8) & 0xff);
                }

                for (int c = 0; c < 4; c++)
                {
                    counterBlock[15 - c - 4] = (byte)(b / 0x100000000 >> c * 8);
                }

                byte[] cipherCntr = this.Cipher(counterBlock, keySchedule);  //// -- encrypt counter block --

                //// block size is reduced on final block
                int blockLength = b < blockCount - 1 ? blockSize : (plaintext.Length - 1) % blockSize + 1;
                char[] cipherChar = new char[blockLength];

                for (int i = 0; i < blockLength; i++)
                {
                    //// -- xor plaintext with ciphered counter char-by-char --
                    cipherChar[i] = Convert.ToChar(cipherCntr[i] ^ Convert.ToInt32(Convert.ToChar(plaintext.Substring(b * blockSize + i, 1).ToString())));
                }

                ciphertxt[b] = new string(cipherChar);
            }

            //// Array.join is more efficient than repeated string concatenation
            string ciphertext = string.Empty;

            for (int i = 0; i < ciphertxt.Length; i++)
            {
                ciphertext += ciphertxt[i].ToString();
            }

            ciphertext = ctrTxt + ciphertext;
            ciphertext = this.encodeBase64(ciphertext);
            ////alert((new Date()) - t);
            return ciphertext;
        }

        /// <summary>
        /// Decrypts string using AES.
        /// </summary>
        /// <param name="ciphertext">The cipher text.</param>
        /// <param name="password">The password.</param>
        /// <param name="noOfBits">The no of bits.</param>
        /// <returns>Returns decrypted string.</returns>
        public string AESDecryptCtr(string ciphertext, string password, int noOfBits)
        {
            int blockSize = 16;  //// block size fixed at 16 bytes / 128 bits (Nb=4) for AES
            ////int nBits = 128;     ////number of bits to be used in the key (128, 192, or 256)
            if (!(noOfBits == 128 || noOfBits == 192 || noOfBits == 256))
            {
                return string.Empty;  //// standard allows 128/192/256 bit keys
            }

            ciphertext = this.decodeBase64(ciphertext);

            ////var t = new Date();  //// timer

            //// use AES to encrypt password (mirroring encrypt routine)
            int noOfBytes = noOfBits / 8;  // no bytes in key
            byte[] passwordBytes = new byte[noOfBytes];

            for (int i = 0; (i < password.Length) && (i < passwordBytes.Length); i++)
            {
                char c = Convert.ToChar(password.Substring(i, 1).ToString());
                passwordBytes[i] = Convert.ToByte(c);
            }

            byte[] key = this.Cipher(passwordBytes, this.KeyExpansion(passwordBytes));
            key = this.expandKey(key, noOfBytes);

            //// recover nonce from 1st 8 bytes of ciphertext
            byte[] counterBlock = new byte[8];
            string ctrTxt = ciphertext.Substring(0, 8);

            for (int i = 0; i < 8; i++)
            {
                counterBlock[i] = (byte)Convert.ToChar(ctrTxt.Substring(i, 1).ToString());
            }

            //// generate key schedule
            byte[][] keySchedule = this.KeyExpansion(key);

            //// separate ciphertext into blocks (skipping past initial 8 bytes)
            int noOfBlocks = (int)Math.Ceiling((double)(ciphertext.Length - 8) / (double)blockSize);
            string[] ct = new string[noOfBlocks];

            for (int b = 0; b < noOfBlocks; b++)
            {
                if ((ciphertext.Length - (8 + b * blockSize)) < blockSize)
                {
                    ct[b] = ciphertext.Substring(8 + b * blockSize, ciphertext.Length - (8 + b * blockSize));
                }
                else
                {
                    ct[b] = ciphertext.Substring(8 + b * blockSize, blockSize);
                }
            }

            string[] arrCiphertext;
            arrCiphertext = ct;

            //// plaintext will get generated block-by-block into array of block-length strings
            ////var plaintxt = new Array(ciphertext.Length);
            string[] plaintxt = new string[arrCiphertext.Length];

            for (int b = 0; b < noOfBlocks; b++)
            {
                //// set counter (block #) in last 8 bytes of counter block (leaving nonce in 1st 8 bytes)
                counterBlock = (byte[])this.ResizeArray(counterBlock, 16);

                for (int c = 0; c < 4; c++)
                {
                    counterBlock[15 - c] = (byte)((b >> c * 8) & 0xff);
                }

                for (int c = 0; c < 4; c++)
                {
                    if (((b + 1) / 0x100000000 - 1) >= 0)
                    {
                        counterBlock[15 - c - 4] = (byte)((((b + 1) / 0x100000000 - 1) >> c * 8) & 0xff);
                    }
                }

                byte[] cipherCntr = this.Cipher(counterBlock, keySchedule);  //// encrypt counter block
                char[] plaintxtByte = new char[arrCiphertext[b].Length];

                for (int i = 0; i < arrCiphertext[b].Length; i++)
                {
                    //// -- xor plaintxt with ciphered counter byte-by-byte --
                    plaintxtByte[i] = Convert.ToChar(cipherCntr[i] ^ Convert.ToInt32(Convert.ToChar(arrCiphertext[b].Substring(i, 1))));
                }

                plaintxt[b] = string.Empty;

                for (int i = 0; i < plaintxtByte.Length; i++)
                {
                    plaintxt[b] += plaintxtByte[i].ToString();
                }
            }

            //// join array of blocks into single plaintext string
            string plaintext = string.Empty;

            for (int i = 0; i < plaintxt.Length; i++)
            {
                plaintext += plaintxt[i];
            }

            return plaintext;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets Cipher for specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="w">The w.</param>
        /// <returns>Returns Cipher byte array.</returns>
        private byte[] Cipher(byte[] input, byte[][] w)
        {    //// main Cipher function [§5.1]
            int noOfBlock = 4;               //// block size (in words): no of columns in state (fixed at 4 for AES)
            int noOfRound = w.Length / noOfBlock - 1; //// no of rounds: 10/12/14 for 128/192/256-bit keys

            byte[,] state = new byte[4, 4];  //// initialise 4xNb byte-array 'state' with input [§3.4]

            for (int i = 0; i < 4 * noOfBlock; i++)
            {
                state[i % 4, (int)Math.Floor((double)i / 4)] = input[i];
            }

            state = this.AddRoundKey(state, w, 0, noOfBlock);

            for (int round = 1; round < noOfRound; round++)
            {
                state = this.SubBytes(state, noOfBlock);
                state = this.ShiftRows(state, noOfBlock);
                state = this.MixColumns(state, noOfBlock);
                state = this.AddRoundKey(state, w, round, noOfBlock);
            }

            state = this.SubBytes(state, noOfBlock);
            state = this.ShiftRows(state, noOfBlock);
            state = this.AddRoundKey(state, w, noOfRound, noOfBlock);

            byte[] output = new byte[4 * noOfBlock];      //// convert state to 1-d array before returning [§3.4]

            for (int i = 0; i < 4 * noOfBlock; i++)
            {
                output[i] = state[i % 4, (int)Math.Floor((double)i / 4)];
            }

            return output;
        }

        /// <summary>
        /// Adds the round key.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="w">The word.</param>
        /// <param name="rnd">The random.</param>
        /// <param name="noOfBlock">The no of blocks.</param>
        /// <returns>Returns round key byte array.</returns>
        private byte[,] AddRoundKey(byte[,] state, byte[][] w, int rnd, int noOfBlock)
        {
            //// xor Round Key into state S [§5.1.4]

            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < noOfBlock; c++)
                {
                    state[r, c] ^= w[rnd * 4 + c][r];
                }
            }

            return state;
        }

        /// <summary>
        /// Subtract the bytes.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="noOfBlock">The no of blocks.</param>
        /// <returns>Returns byte array.</returns>
        private byte[,] SubBytes(byte[,] s, int noOfBlock)
        {    //// apply SBox to state S [§5.1.1]
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < noOfBlock; c++)
                {
                    s[r, c] = this.sbox[s[r, c]];
                }
            }

            return s;
        }

        /// <summary>
        /// Shifts the rows.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="noOfBlock">The no of blocks.</param>
        /// <returns>Returns byte array.</returns>
        private byte[,] ShiftRows(byte[,] s, int noOfBlock)
        {    //// shift row r of state S left by r bytes [§5.1.2]
            byte[] t = new byte[4];

            for (int r = 1; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    t[c] = s[r, (c + r) % noOfBlock];  //// shift into temp copy
                }

                for (int c = 0; c < 4; c++)
                {
                    s[r, c] = t[c];         //// and copy back
                }
            }          //// note that this will work for Nb=4,5,6, but not 7,8 (always 4 for AES):

            return s;  //// see fp.gladman.plus.com/cryptography_technology/rijndael/aes.spec.311.pdf 
        }

        /// <summary>
        /// Mixes the columns.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="noOfBlock">The no of blocks.</param>
        /// <returns>Returns byte array.</returns>
        private byte[,] MixColumns(byte[,] s, int noOfBlock)
        {
            //// combine bytes of each col of state S [§5.1.3]
            for (int c = 0; c < 4; c++)
            {
                byte[] a = new byte[4];  //// 'a' is a copy of the current column from 's'
                byte[] b = new byte[4];  //// 'b' is a•{02} in GF(2^8)

                for (int i = 0; i < 4; i++)
                {
                    a[i] = s[i, c];
                    b[i] = (byte)(Convert.ToBoolean(s[i, c] & 0x80) ? s[i, c] << 1 ^ 0x011b : s[i, c] << 1);
                }

                //// a[n] ^ b[n] is a•{03} in GF(2^8)
                s[0, c] = (byte)(b[0] ^ a[1] ^ b[1] ^ a[2] ^ a[3]); //// 2*a0 + 3*a1 + a2 + a3
                s[1, c] = (byte)(a[0] ^ b[1] ^ a[2] ^ b[2] ^ a[3]); //// a0 * 2*a1 + 3*a2 + a3
                s[2, c] = (byte)(a[0] ^ a[1] ^ b[2] ^ a[3] ^ b[3]); //// a0 + a1 + 2*a2 + 3*a3
                s[3, c] = (byte)(a[0] ^ b[0] ^ a[1] ^ a[2] ^ b[3]); //// 3*a0 + a1 + a2 + 2*a3
            }

            return s;
        }

        /// <summary>
        /// Expand the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Returns byte array.</returns>
        private byte[][] KeyExpansion(byte[] key)
        {
            //// generate Key Schedule (byte-array Nr+1 x Nb) from Key [§5.2]
            int noOfBlock = 4;            //// block size (in words): no of columns in state (fixed at 4 for AES)
            int noOfKey = key.Length / 4;  //// key length (in words): 4/6/8 for 128/192/256-bit keys
            int noOfRound = noOfKey + 6;       //// no of rounds: 10/12/14 for 128/192/256-bit keys

            byte[][] w = new byte[noOfBlock * (noOfRound + 1)][];
            byte[] temp = new byte[4];

            for (int i = 0; i < noOfKey; i++)
            {
                byte[] r = { key[4 * i], key[4 * i + 1], key[4 * i + 2], key[4 * i + 3] };
                w[i] = r;
            }

            for (int i = noOfKey; i < (noOfBlock * (noOfRound + 1)); i++)
            {
                w[i] = new byte[4];

                for (int t = 0; t < 4; t++)
                {
                    temp[t] = w[i - 1][t];
                }

                if (i % noOfKey == 0)
                {
                    temp = this.SubWord(this.RotWord(temp));

                    for (int t = 0; t < 4; t++)
                    {
                        temp[t] ^= this.rcon[i / noOfKey, t];
                    }
                }
                else if (noOfKey > 6 && i % noOfKey == 4)
                {
                    temp = this.SubWord(temp);
                }

                for (int t = 0; t < 4; t++)
                {
                    w[i][t] = (byte)((int)w[i - noOfKey][t] ^ (int)temp[t]);
                }
            }

            return w;
        }

        /// <summary>
        /// Subtract the word.
        /// </summary>
        /// <param name="w">The word.</param>
        /// <returns>Returns byte array.</returns>
        private byte[] SubWord(byte[] w)
        {
            //// apply SBox to 4-byte word w
            for (int i = 0; i < 4; i++)
            {
                w[i] = this.sbox[w[i]];
            }

            return w;
        }

        /// <summary>
        /// Rotates the word.
        /// </summary>
        /// <param name="w">The word.</param>
        /// <returns>Returns byte array.</returns>
        private byte[] RotWord(byte[] w)
        {
            //// rotate 4-byte word w left by one byte
            byte tmp = w[0];

            for (int i = 0; i < 3; i++)
            {
                w[i] = w[i + 1];
            }

            w[3] = tmp;

            return w;
        }

        /// <summary>
        /// Encodes the string using base64.
        /// </summary>
        /// <param name="ciphertext">The cipher text.</param>
        /// <returns>Returns encoded string.</returns>
        private string encodeBase64(string ciphertext)
        {
            int c;
            int o1, o2, o3, bits, h1, h2, h3, h4;
            System.Collections.ArrayList e = new System.Collections.ArrayList();
            string plain, coded, pad = string.Empty;
            plain = ciphertext;

            c = plain.Length % 3;  //// pad string to length of multiple of 3

            if (c > 0)
            {
                while (c++ < 3)
                {
                    pad += '=';
                    plain += '\0';
                }
            }

            for (c = 0; c < plain.Length; c += 3)
            {
                //// pack three octets into four hexets
                o1 = Convert.ToChar(plain.Substring(c, 1).ToString());
                o2 = Convert.ToChar(plain.Substring(c + 1, 1).ToString());
                o3 = Convert.ToChar(plain.Substring(c + 2, 1).ToString());

                bits = o1 << 16 | o2 << 8 | o3;
                h1 = bits >> 18 & 0x3f;
                h2 = bits >> 12 & 0x3f;
                h3 = bits >> 6 & 0x3f;
                h4 = bits & 0x3f;

                //// use hextets to index into b64 string
                string str;
                str = this.b64.Substring(h1, 1).ToString() + this.b64.Substring(h2, 1).ToString() + this.b64.Substring(h3, 1).ToString() + this.b64.Substring(h4, 1).ToString();
                e.Insert(c / 3, str);
            }

            coded = string.Empty; //// join() is far faster than repeated string concatenation

            for (int i = 0; i < e.Count; i++)
            {
                coded += e[i];
            }

            //// replace 'A's from padded nulls with '='s
            coded = coded.Substring(0, coded.Length - pad.Length) + pad;
            return coded;
        }

        /// <summary>
        /// Decodes the string using base64.
        /// </summary>
        /// <param name="ciphertext">The cipher text.</param>
        /// <returns>Returns decoded string.</returns>
        private string decodeBase64(string ciphertext)
        {
            int o1, o2, o3, h1, h2, h3, h4, bits;
            System.Collections.ArrayList d = new System.Collections.ArrayList();
            string plain, coded;

            coded = ciphertext;

            for (int c = 0; c < coded.Length; c += 4)
            {
                h1 = this.b64.IndexOf(coded.Substring(c, 1).ToString());
                h2 = this.b64.IndexOf(coded.Substring(c + 1, 1).ToString());
                h3 = this.b64.IndexOf(coded.Substring(c + 2, 1).ToString());
                h4 = this.b64.IndexOf(coded.Substring(c + 3, 1).ToString());

                bits = h1 << 18 | h2 << 12 | h3 << 6 | h4;

                o1 = bits >> 16 & 0xff;
                o2 = bits >> 8 & 0xff;
                o3 = bits & 0xff;

                string str = Convert.ToChar(o1).ToString() + Convert.ToChar(o2).ToString() + Convert.ToChar(o3).ToString();
                d.Insert(c / 4, str);

                //// check for padding
                if (h4 == 0x40)
                {
                    d[c / 4] = Convert.ToChar(o1).ToString() + Convert.ToChar(o2).ToString();
                }

                if (h3 == 0x40)
                {
                    d[c / 4] = Convert.ToChar(o1).ToString();
                }
            }

            plain = string.Empty;

            for (int i = 0; i < d.Count; i++)
            {
                plain += d[i].ToString();
            }

            return plain;
        }

        /// <summary>
        /// Gets the time.
        /// </summary>
        /// <returns>Returns time in integer value.</returns>
        private long GetTime()
        {
            long retval = 0;
            DateTime st = new DateTime(1970, 1, 1);
            TimeSpan t = DateTime.Now - st;
            retval = (long)t.TotalMilliseconds;

            return retval;
        }

        /// <summary>
        /// Resizes the array.
        /// </summary>
        /// <param name="oldArray">The old array.</param>
        /// <param name="newSize">The new size.</param>
        /// <returns>Returns array.</returns>
        private System.Array ResizeArray(System.Array oldArray, int newSize)
        {
            int oldSize = oldArray.Length;
            System.Type elementType = oldArray.GetType().GetElementType();
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            int preserveLength = System.Math.Min(oldSize, newSize);

            if (preserveLength > 0)
            {
                System.Array.Copy(oldArray, newArray, preserveLength);
            }

            return newArray;
        }

        /// <summary>
        /// Expands the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="newKeyLength">New length of the key.</param>
        /// <returns>Returns byte array.</returns>
        private byte[] expandKey(byte[] key, int newKeyLength)
        {
            byte[] newKey = new byte[newKeyLength];

            for (int i = 0; i < key.Length; i++)
            {
                newKey[i] = key[i];
            }

            for (int i = 0, j = key.Length; j < newKey.Length; i++, j++)
            {
                newKey[j] = key[i];
            }

            return newKey;
        }

        #endregion
    }
}
