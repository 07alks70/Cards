using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System;
using Org.BouncyCastle.OpenSsl;
using System.Threading.Tasks;
using System.Net;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Cards
{
    public class Card
    {
        protected long number;
        protected int cvv;
        protected int exp_month;
        protected int exp_year;
        protected string encrypted_card;

        protected string encripted_key_url = "https://secure.sandbox.symoco.com/encryptkey";
        private string encryptKey;

        protected bool _isGetEncryptedKey = false;

        public Card(int cvv, int exp_month, int exp_year, long number)
        {
            this.cvv = cvv;
            this.exp_month = exp_month;
            this.exp_year = exp_year;
            this.number = number;
        }

        public bool goEncrypt()
        {
            return true;
        }

        public async Task getEncryptKey()
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync("https://secure.sandbox.symoco.com/encryptkey");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                HttpContent responseContent = response.Content;

                var jsonObject = JObject.Parse(await responseContent.ReadAsStringAsync());

                this.encryptKey = jsonObject.SelectToken("pem").ToString();

            }
            else 
            {
                Console.WriteLine("fff ");
            }
        }
        
        private string getCardJsonObject()
        {
            return "{" +
                "'pan':" + this.number + "," +
                "'cvv':" + this.cvv + "," +
                "'exp_month':" + this.exp_month + "," +
                "'exp_year':" + this.exp_year +
                "}";
        }

        private StreamReader stringToStreamReader(string text)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(text); ;
            MemoryStream stream = new MemoryStream(byteArray);
            return new StreamReader(stream);
        }

        public string encrypt()
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(this.getCardJsonObject());

            PemReader pr = new PemReader(this.stringToStreamReader(encryptKey));
            RsaKeyParameters keys = (RsaKeyParameters)pr.ReadObject();

            OaepEncoding eng = new OaepEncoding(new RsaEngine());
            eng.Init(true, keys);

            int length = plainTextBytes.Length;
            int blockSize = eng.GetInputBlockSize();
            List<byte> cipherTextBytes = new List<byte>();
            for (int chunkPosition = 0;
                chunkPosition < length;
                chunkPosition += blockSize)
            {
                int chunkSize = Math.Min(blockSize, length - chunkPosition);
                cipherTextBytes.AddRange(eng.ProcessBlock(
                    plainTextBytes, chunkPosition, chunkSize
                ));
            }

            this.encrypted_card = Convert.ToBase64String(cipherTextBytes.ToArray());

            return this.encrypted_card;
        }

    }
}
