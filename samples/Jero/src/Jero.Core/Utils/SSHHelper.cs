


using Renci.SshNet;
namespace Jero.Core
{
    /// <summary>
    /// SSH/Sftp 工具类
    /// </summary>
    public class SSHHelper : IDisposable
    {
        private readonly SftpClient _sftp;

        public SSHHelper(string host, int port, string user, string password)
        {
            _sftp = new SftpClient(host, port, user, password);
        }

        /// <summary>
        /// 连接
        /// </summary>
        private void Connect()
        {
            if (!_sftp.IsConnected)
                _sftp.Connect();
        }

        /// <summary>
        /// 是否存在同名文件
        /// </summary>
        /// <param name="ftpFileName"></param>
        /// <returns></returns>
        public bool Exists(string ftpFileName)
        {
            Connect();

            return _sftp.Exists(ftpFileName);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="ftpFileName"></param>
        public void DeleteFile(string ftpFileName)
        {
            Connect();

            _sftp.DeleteFile(ftpFileName);
        }

        /// <summary>
        /// 下载到指定目录
        /// </summary>
        /// <param name="ftpFileName"></param>
        /// <param name="localFileName"></param>
        public void DownloadFile(string ftpFileName, string localFileName)
        {
            Connect();

            using (Stream fileStream = File.OpenWrite(localFileName))
            {
                _sftp.DownloadFile(ftpFileName, fileStream);
            }
        }

        /// <summary>
        /// 读取字节
        /// </summary>
        /// <param name="ftpFileName"></param>
        /// <returns></returns>
        public byte[] ReadAllBytes(string ftpFileName)
        {
            Connect();

            return _sftp.ReadAllBytes(ftpFileName);
        }

        /// <summary>
        /// 读取流
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Stream OpenRead(string path)
        {
            return _sftp.Open(path, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// 继续下载
        /// </summary>
        /// <param name="ftpFileName"></param>
        /// <param name="localFileName"></param>
        public void DownloadFileWithResume(string ftpFileName, string localFileName)
        {
            DownloadFile(ftpFileName, localFileName);
        }

        /// <summary>
        /// 重命名
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        public void RenameFile(string oldPath, string newPath)
        {
            _sftp.RenameFile(oldPath, newPath);
        }

        /// <summary>
        /// 指定目录下文件
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public List<string> GetFileList(string folder, IEnumerable<string> filters)
        {
            Connect();

            var files = new List<string>();
            var sftpFiles = _sftp.ListDirectory(folder);
            foreach (var file in sftpFiles)
            {
                if (file.IsRegularFile && filters.Any(f => file.Name.EndsWith(f)))
                    files.Add(file.Name);
            }
            return files;
        }

        /// <summary>
        /// 上传指定目录文件
        /// </summary>
        /// <param name="localFileName"></param>
        /// <param name="ftpFileName"></param>
        public void UploadFile(string localFileName, string ftpFileName)
        {
            Connect();

            var dir = Path.GetDirectoryName(ftpFileName);
            CreateDir(_sftp, dir);
            using (var fileStream = new FileStream(localFileName, FileMode.Open))
            {
                _sftp.UploadFile(fileStream, ftpFileName);
            }
        }

        /// <summary>
        /// 上传字节
        /// </summary>
        /// <param name="bs"></param>
        /// <param name="ftpFileName"></param>
        public void UploadFile(byte[] bs, string ftpFileName)
        {
            Connect();

            var dir = Path.GetDirectoryName(ftpFileName);
            CreateDir(_sftp, dir);
            _sftp.WriteAllBytes(ftpFileName, bs);
        }

        /// <summary>
        /// 上传流
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="ftpFileName"></param>
        public void UploadFile(Stream fileStream, string ftpFileName)
        {
            Connect();

            var dir = Path.GetDirectoryName(ftpFileName);
            CreateDir(_sftp, dir);
            _sftp.UploadFile(fileStream, ftpFileName);
            fileStream.Dispose();
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="sftp"></param>
        /// <param name="dir"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void CreateDir(SftpClient sftp, string? dir)
        {
            if (string.IsNullOrWhiteSpace(dir)) return;

            if (sftp.Exists(dir)) return;

            var index = dir.LastIndexOfAny(new char[] { '/', '\\' });
            if (index > 0)
            {
                var p = dir[..index];
                if (!sftp.Exists(p))
                    CreateDir(sftp, p);
                sftp.CreateDirectory(dir);
            }
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        public void Dispose()
        {
            if (_sftp == null) return;

            if (_sftp.IsConnected)
                _sftp.Disconnect();
            _sftp.Dispose();
        }
    }
}
