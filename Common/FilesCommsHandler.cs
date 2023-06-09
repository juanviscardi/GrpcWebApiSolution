﻿using System.Net.Sockets;

namespace Common
{
    public class FileCommsHandler
    {
        private readonly ConversionHandler _conversionHandler;
        private readonly FileHandler _fileHandler;
        private readonly FileStreamHandler _fileStreamHandler;
        private readonly SocketHelper _socketHelper;

        public FileCommsHandler(TcpClient tcpClient)
        {
            _conversionHandler = new ConversionHandler();
            _fileHandler = new FileHandler();
            _fileStreamHandler = new FileStreamHandler();
            _socketHelper = new SocketHelper(tcpClient);
        }

        public async Task SendFile(string path)
        {
            if (await _fileHandler.FileExists(path))
            {
                var fileName = await _fileHandler.GetFileName(path);
                // ---> Enviar el largo del nombre del archivo
                await _socketHelper.Send(_conversionHandler.ConvertIntToBytes(fileName.Length));
                // ---> Enviar el nombre del archivo
                await _socketHelper.Send(_conversionHandler.ConvertStringToBytes(fileName));

                // ---> Obtener el tamaño del archivo
                long fileSize = await _fileHandler.GetFileSize(path);
                // ---> Enviar el tamaño del archivo
                var convertedFileSize = _conversionHandler.ConvertLongToBytes(fileSize);
                await _socketHelper.Send(convertedFileSize);
                // ---> Enviar el archivo (pero con file stream)
                await SendFileWithStream(fileSize, path);
            }
            else
            {
                throw new Exception("File does not exist");
            }
        }

        public async Task<string> ReceiveFile()
        {
            // ---> Recibir el largo del nombre del archivo
            int fileNameSize = _conversionHandler.ConvertBytesToInt(
               await _socketHelper.Receive(ProtocolSpecification.FixedDataSize));
            // ---> Recibir el nombre del archivo
            string fileName = _conversionHandler.ConvertBytesToString(await _socketHelper.Receive(fileNameSize));
            // ---> Recibir el largo del archivo
            long fileSize = _conversionHandler.ConvertBytesToLong(
                await _socketHelper.Receive(ProtocolSpecification.FixedFileSize));
            // ---> Recibir el archivo
            await ReceiveFileWithStreams(fileSize, fileName);
            return fileName;
        }

        private async Task SendFileWithStream(long fileSize, string path)
        {
            long fileParts = await ProtocolSpecification.CalculateFileParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            //Mientras tengo un segmento a enviar
            while (fileSize > offset)
            {
                byte[] data;
                //Es el último segmento?
                if (currentPart == fileParts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    //1- Leo de disco el último segmento
                    //2- Guardo el último segmento en un buffer
                    data = await _fileStreamHandler.Read(path, offset, lastPartSize); //Puntos 1 y 2
                    offset += lastPartSize;
                }
                else
                {
                    //1- Leo de disco el segmento
                    //2- Guardo ese segmento en un buffer
                    data = await _fileStreamHandler.Read(path, offset, ProtocolSpecification.MaxPacketSize);
                    offset += ProtocolSpecification.MaxPacketSize;
                }

                await _socketHelper.Send(data); //3- Envío ese segmento a travez de la red
                currentPart++;
            }
        }

        private async Task ReceiveFileWithStreams(long fileSize, string fileName)
        {
            long fileParts = await ProtocolSpecification.CalculateFileParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            //Mientras tengo partes para recibir
            while (fileSize > offset)
            {
                byte[] data;
                //1- Me fijo si es la ultima parte
                if (currentPart == fileParts)
                {
                    //1.1 - Si es, recibo la ultima parte
                    var lastPartSize = (int)(fileSize - offset);
                    data = await _socketHelper.Receive(lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    //2.2- Si no, recibo una parte cualquiera
                    data = await _socketHelper.Receive(ProtocolSpecification.MaxPacketSize);
                    offset += ProtocolSpecification.MaxPacketSize;
                }
                //3- Escribo esa parte del archivo a disco
                await _fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
        }

        /*public FileCommsHandler(Socket socket)
        {
            _conversionHandler = new ConversionHandler();
            _fileHandler = new FileHandler();
            _fileStreamHandler = new FileStreamHandler();
            _socketHelper = new SocketHelper(socket);
        }

        public void SendFile(string path)
        {
            if (_fileHandler.FileExists(path))
            {
                var fileName = _fileHandler.GetFileName(path);
                // ---> Enviar el largo del nombre del archivo
                _socketHelper.Send(_conversionHandler.ConvertIntToBytes(fileName.Length));
                // ---> Enviar el nombre del archivo
                _socketHelper.Send(_conversionHandler.ConvertStringToBytes(fileName));

                // ---> Obtener el tamaño del archivo
                long fileSize = _fileHandler.GetFileSize(path);
                // ---> Enviar el tamaño del archivo
                var convertedFileSize = _conversionHandler.ConvertLongToBytes(fileSize);
                _socketHelper.Send(convertedFileSize);
                // ---> Enviar el archivo (pero con file stream)
                SendFileWithStream(fileSize, path);
            }
            else
            {
                throw new Exception("File does not exist");
            }
        }

        public string ReceiveFile()
        {
            // ---> Recibir el largo del nombre del archivo
            int fileNameSize = _conversionHandler.ConvertBytesToInt(
                _socketHelper.Receive(ProtocolSpecification.FixedDataSize));
            // ---> Recibir el nombre del archivo
            string fileName = _conversionHandler.ConvertBytesToString(_socketHelper.Receive(fileNameSize));
            // ---> Recibir el largo del archivo
            long fileSize = _conversionHandler.ConvertBytesToLong(
                _socketHelper.Receive(ProtocolSpecification.FixedFileSize));
            // ---> Recibir el archivo
            ReceiveFileWithStreams(fileSize, fileName);
            return fileName;
        }

        private void SendFileWithStream(long fileSize, string path)
        {
            long fileParts = ProtocolSpecification.CalculateFileParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            //Mientras tengo un segmento a enviar
            while (fileSize > offset)
            {
                byte[] data;
                //Es el último segmento?
                if (currentPart == fileParts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    //1- Leo de disco el último segmento
                    //2- Guardo el último segmento en un buffer
                    data = _fileStreamHandler.Read(path, offset, lastPartSize); //Puntos 1 y 2
                    offset += lastPartSize;
                }
                else
                {
                    //1- Leo de disco el segmento
                    //2- Guardo ese segmento en un buffer
                    data = _fileStreamHandler.Read(path, offset, ProtocolSpecification.MaxPacketSize);
                    offset += ProtocolSpecification.MaxPacketSize;
                }

                _socketHelper.Send(data); //3- Envío ese segmento a travez de la red
                currentPart++;
            }
        }

        private void ReceiveFileWithStreams(long fileSize, string fileName)
        {
            long fileParts = ProtocolSpecification.CalculateFileParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            //Mientras tengo partes para recibir
            while (fileSize > offset)
            {
                byte[] data;
                //1- Me fijo si es la ultima parte
                if (currentPart == fileParts)
                {
                    //1.1 - Si es, recibo la ultima parte
                    var lastPartSize = (int)(fileSize - offset);
                    data = _socketHelper.Receive(lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    //2.2- Si no, recibo una parte cualquiera
                    data = _socketHelper.Receive(ProtocolSpecification.MaxPacketSize);
                    offset += ProtocolSpecification.MaxPacketSize;
                }
                //3- Escribo esa parte del archivo a disco
                _fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
        }*/


    }
}
