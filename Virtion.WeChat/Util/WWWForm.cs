using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Virtion.WeChat.Util
{
    /// <summary>
    ///   <para>Helper class to generate form data to post to web servers using the WWW class.</para>
    /// </summary>
    public sealed class WWWForm
    {
        private List<byte[]> formData;

        private List<string> fieldNames;

        private List<string> fileNames;

        private List<string> types;

        private byte[] boundary;

        private bool containsFiles;

        /// <summary>
        ///   <para>(Read Only) Returns the correct request headers for posting the form using the WWW class.</para>
        /// </summary>
        public Dictionary<string, string> headers
        {
            get
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                if (this.containsFiles)
                {
                    dictionary["Content-Type"] = "multipart/form-data; boundary=\"" + Encoding.UTF8.GetString(this.boundary, 0, this.boundary.Length) + "\"";
                }
                else
                {
                    dictionary["Content-Type"] = "application/x-www-form-urlencoded";
                }
                return dictionary;
            }
        }

        /// <summary>
        ///   <para>(Read Only) The raw data to pass as the POST request body when sending the form.</para>
        /// </summary>
        public byte[] data
        {
            get
            {
                byte[] result;
                if (this.containsFiles)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes("--");
                    byte[] bytes2 = Encoding.UTF8.GetBytes("\r\n");
                    byte[] bytes3 = Encoding.UTF8.GetBytes("Content-Type: ");
                    byte[] bytes4 = Encoding.UTF8.GetBytes("Content-disposition: form-data; name=\"");
                    byte[] bytes5 = Encoding.UTF8.GetBytes("\"");
                    byte[] bytes6 = Encoding.UTF8.GetBytes("; filename=\"");
                    using (MemoryStream memoryStream = new MemoryStream(1024))
                    {
                        for (int i = 0; i < this.formData.Count; i++)
                        {
                            memoryStream.Write(bytes2, 0, bytes2.Length);
                            memoryStream.Write(bytes, 0, bytes.Length);
                            memoryStream.Write(this.boundary, 0, this.boundary.Length);
                            memoryStream.Write(bytes2, 0, bytes2.Length);
                            memoryStream.Write(bytes3, 0, bytes3.Length);
                            byte[] bytes7 = Encoding.UTF8.GetBytes(this.types[i]);
                            memoryStream.Write(bytes7, 0, bytes7.Length);
                            memoryStream.Write(bytes2, 0, bytes2.Length);
                            memoryStream.Write(bytes4, 0, bytes4.Length);
                            string headerName = Encoding.UTF8.HeaderName;
                            string text = this.fieldNames[i];
                            if (!WWWTranscoder.SevenBitClean(text, Encoding.UTF8) || text.IndexOf("=?") > -1)
                            {
                                text = string.Concat(new string[]
								{
									"=?",
									headerName,
									"?Q?",
									WWWTranscoder.QPEncode(text, Encoding.UTF8),
									"?="
								});
                            }
                            byte[] bytes8 = Encoding.UTF8.GetBytes(text);
                            memoryStream.Write(bytes8, 0, bytes8.Length);
                            memoryStream.Write(bytes5, 0, bytes5.Length);
                            if (this.fileNames[i] != null)
                            {
                                string text2 = this.fileNames[i];
                                if (!WWWTranscoder.SevenBitClean(text2, Encoding.UTF8) || text2.IndexOf("=?") > -1)
                                {
                                    text2 = string.Concat(new string[]
									{
										"=?",
										headerName,
										"?Q?",
										WWWTranscoder.QPEncode(text2, Encoding.UTF8),
										"?="
									});
                                }
                                byte[] bytes9 = Encoding.UTF8.GetBytes(text2);
                                memoryStream.Write(bytes6, 0, bytes6.Length);
                                memoryStream.Write(bytes9, 0, bytes9.Length);
                                memoryStream.Write(bytes5, 0, bytes5.Length);
                            }
                            memoryStream.Write(bytes2, 0, bytes2.Length);
                            memoryStream.Write(bytes2, 0, bytes2.Length);
                            byte[] array = this.formData[i];
                            memoryStream.Write(array, 0, array.Length);
                        }
                        memoryStream.Write(bytes2, 0, bytes2.Length);
                        memoryStream.Write(bytes, 0, bytes.Length);
                        memoryStream.Write(this.boundary, 0, this.boundary.Length);
                        memoryStream.Write(bytes, 0, bytes.Length);
                        memoryStream.Write(bytes2, 0, bytes2.Length);
                        result = memoryStream.ToArray();
                        return result;
                    }
                }
                byte[] bytes10 = Encoding.UTF8.GetBytes("&");
                byte[] bytes11 = Encoding.UTF8.GetBytes("=");
                using (MemoryStream memoryStream2 = new MemoryStream(1024))
                {
                    for (int j = 0; j < this.formData.Count; j++)
                    {
                        byte[] array2 = WWWTranscoder.URLEncode(Encoding.UTF8.GetBytes(this.fieldNames[j]));
                        byte[] toEncode = this.formData[j];
                        byte[] array3 = WWWTranscoder.URLEncode(toEncode);
                        if (j > 0)
                        {
                            memoryStream2.Write(bytes10, 0, bytes10.Length);
                        }
                        memoryStream2.Write(array2, 0, array2.Length);
                        memoryStream2.Write(bytes11, 0, bytes11.Length);
                        memoryStream2.Write(array3, 0, array3.Length);
                    }
                    result = memoryStream2.ToArray();
                }
                return result;
            }
        }

        /// <summary>
        ///   <para>Creates an empty WWWForm object.</para>
        /// </summary>
        public WWWForm()
        {
            this.formData = new List<byte[]>();
            this.fieldNames = new List<string>();
            this.fileNames = new List<string>();
            this.types = new List<string>();
            this.boundary = new byte[40];
            for (int i = 0; i < 40; i++)
            {
                var r = new Random();
                int num = r.Next(48, 110);
                if (num > 57)
                {
                    num += 7;
                }
                if (num > 90)
                {
                    num += 6;
                }
                this.boundary[i] = (byte)num;
            }
        }

        /// <summary>
        ///   <para>Add a simple field to the form.</para>
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="e"></param>
        public void AddField(string fieldName, string value)
        {
            Encoding uTF = Encoding.UTF8;
            this.AddField(fieldName, value, uTF);
        }

        /// <summary>
        ///   <para>Add a simple field to the form.</para>
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="e"></param>
        public void AddField(string fieldName, string value,Encoding e)
        {
            this.fieldNames.Add(fieldName);
            this.fileNames.Add(null);
            this.formData.Add(e.GetBytes(value));
            this.types.Add("text/plain; charset=\"" + e.WebName + "\"");
        }

        /// <summary>
        ///   <para>Adds a simple field to the form.</para>
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="i"></param>
        public void AddField(string fieldName, int i)
        {
            this.AddField(fieldName, i.ToString());
        }

        /// <summary>
        ///   <para>Add binary data to the form.</para>
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="contents"></param>
        /// <param name="fileName"></param>
        /// <param name="mimeType"></param>
        public void AddBinaryData(string fieldName, byte[] contents, string fileName)
        {
            string mimeType = null;
            this.AddBinaryData(fieldName, contents, fileName, mimeType);
        }

        /// <summary>
        ///   <para>Add binary data to the form.</para>
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="contents"></param>
        /// <param name="fileName"></param>
        /// <param name="mimeType"></param>
        public void AddBinaryData(string fieldName, byte[] contents)
        {
            string mimeType = null;
            string fileName = null;
            this.AddBinaryData(fieldName, contents, fileName, mimeType);
        }

        /// <summary>
        ///   <para>Add binary data to the form.</para>
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="contents"></param>
        /// <param name="fileName"></param>
        /// <param name="mimeType"></param>
        public void AddBinaryData(string fieldName, byte[] contents,  string fileName, string mimeType)
        {
            this.containsFiles = true;
            bool flag = contents.Length > 8 && contents[0] == 137 && contents[1] == 80 && contents[2] == 78 && contents[3] == 71 && contents[4] == 13 && contents[5] == 10 && contents[6] == 26 && contents[7] == 10;
            if (fileName == null)
            {
                fileName = fieldName + ((!flag) ? ".dat" : ".png");
            }
            if (mimeType == null)
            {
                if (flag)
                {
                    mimeType = "image/png";
                }
                else
                {
                    mimeType = "application/octet-stream";
                }
            }
            this.fieldNames.Add(fieldName);
            this.fileNames.Add(fileName);
            this.formData.Add(contents);
            this.types.Add(mimeType);
        }
    }
}
