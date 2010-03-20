using System;
using System.IO;

namespace LogEm
{
    public class ManifestResourceHelper
    {
        public static void WriteResourceToStream(Stream outputStream, string resourceName)
        {
            // Grab the resource stream from the manifest.
            Type thisType = typeof(ManifestResourceHelper);

            using (Stream inputStream = thisType.Assembly.GetManifestResourceStream(resourceName))
            {
                // Allocate a buffer for reading the stream. The maximum size
                // of this buffer is fixed to 4 KB.
                byte[] buffer = new byte[Math.Min(inputStream.Length, 4096)];

                // Finally, write out the bytes!
                int readLength = inputStream.Read(buffer, 0, buffer.Length);
                while (readLength > 0)
                {
                    outputStream.Write(buffer, 0, readLength);
                    readLength = inputStream.Read(buffer, 0, buffer.Length);
                }
            }
        }
    }
}
