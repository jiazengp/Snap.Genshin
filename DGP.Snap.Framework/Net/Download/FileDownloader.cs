using DGP.Snap.Framework.Extensions.System;
using DGP.Snap.Framework.Extensions.System.Net;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;

namespace DGP.Snap.Framework.Net.Download
{
    /// <summary>
    /// Class used for downloading files. The .NET WebClient is used for downloading.
    /// </summary>
    public class FileDownloader : IFileDownloader
    {
        private readonly IDownloadCache downloadCache;
        private readonly ManualResetEvent readyToDownload = new ManualResetEvent(true);
        private readonly System.Timers.Timer attemptTimer = new System.Timers.Timer();
        private readonly object cancelSync = new object();

        private bool isCancelled;
        private bool disposed;
        private bool useFileNameFromServer = true;
        private bool isFallback;

        private int attemptNumber;

        private string localFileName;
        private string destinationFileName;
        private string destinationFolder;

        private Uri fileSource;
        private StreamCopyWorker worker;
        private DownloadWebClient downloadWebClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDownloader"/> class. No download cache would be used, resume is not supported
        /// </summary>
        public FileDownloader()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDownloader"/> class.
        /// </summary>
        /// <param name="downloadCache">IDownloadCache instance</param>
        public FileDownloader(IDownloadCache downloadCache)
        {
            this.DnsFallbackResolver = null;

            this.MaxAttempts = 60;
            this.DelayBetweenAttempts = TimeSpan.FromSeconds(3);
            this.SafeWaitTimeout = TimeSpan.FromSeconds(15);
            this.SourceStreamReadTimeout = TimeSpan.FromSeconds(5);

            this.downloadCache = downloadCache;
            this.disposed = false;

            this.attemptTimer.Elapsed += OnDownloadAttemptTimer;
        }

        /// <summary>
        /// Fired when download is finished, even if it's failed.
        /// </summary>
        public event EventHandler<DownloadFileCompletedArgs> DownloadFileCompleted;

        /// <summary>
        /// Fired when download progress is changed.
        /// </summary>
        public event EventHandler<DownloadFileProgressChangedArgs> DownloadProgressChanged;

        /// <summary>
        /// Gets or sets the DNS fallback resolver. Default is null.
        /// </summary>
        public IDnsFallbackResolver DnsFallbackResolver { get; set; }

        /// <summary>
        /// Gets or sets the delay between download attempts. Default is 3 seconds. 
        /// </summary>
        public TimeSpan DelayBetweenAttempts { get; set; }

        /// <summary>
        /// Gets or sets the maximum waiting timeout for pending request to be finished. Default is 15 seconds.
        /// </summary>
        public TimeSpan SafeWaitTimeout { get; set; }

        /// <summary>
        /// Gets or sets the timeout for source stream. Default is 5 seconds.
        /// </summary>
        public TimeSpan SourceStreamReadTimeout { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of download attempts. Default is 60.
        /// </summary>
        public int MaxAttempts { get; set; }

        /// <summary>
        /// Gets the total bytes received so far
        /// </summary>
        public long BytesReceived { get; internal set; }

        /// <summary>
        /// Gets the total bytes to receive
        /// </summary>
        public long TotalBytesToReceive { get; internal set; }

        /// <summary>
        /// Gets or sets the time when download was started
        /// </summary>
        public DateTime DownloadStartTime { get; set; }

        private bool UseCaching => this.downloadCache != null;

        /// <summary>
        /// Start async download of source to destinationPath
        /// </summary>
        /// <param name="source">Source URI</param>
        /// <param name="destinationPath">Full path with file name.</param>
        public void DownloadFileAsync(Uri source, string destinationPath) => DownloadFileAsync(source, destinationPath, false);

        /// <summary>
        /// Start download of source file to downloadDirectory. File would be saved with filename taken from server 
        /// </summary>
        /// <param name="source">Source URI</param>
        /// <param name="destinationDirectory">Destination directory</param>
        public void DownloadFileAsyncPreserveServerFileName(Uri source, string destinationDirectory) => DownloadFileAsync(source, Path.Combine(destinationDirectory, Guid.NewGuid().ToString()), true);

        /// <summary>
        /// Cancel current download
        /// </summary>
        public void CancelDownloadAsync()
        {
            lock (this.cancelSync)
            {
                if (this.isCancelled)
                {
                    return;
                }
                this.isCancelled = true;
            }

            if (this.worker != null)
            {
                this.worker.Cancel();
            }

            TriggerDownloadWebClientCancelAsync();
            DeleteDownloadedFile();  ////todo: maybe this is equal to InvalidateCache? Can we get rid of DeleteDownloadedFile ?

            this.readyToDownload.Set();
        }

        private void DeleteDownloadedFile() => FileUtils.TryFileDelete(this.localFileName);

        private void InvalidateCache(Uri uri)
        {
            if (!this.UseCaching)
            {
                return;
            }

            this.downloadCache.Invalidate(uri);
        }

        private void DownloadFileAsync(Uri source, string destinationPath, bool useServerFileName)
        {
            if (!WaitSafeStart())
            {
                throw new Exception("Unable to start download because another request is still in progress.");
            }

            this.useFileNameFromServer = useServerFileName;
            this.fileSource = source;
            this.BytesReceived = 0;
            this.destinationFileName = destinationPath;
            this.destinationFolder = Path.GetDirectoryName(destinationPath);
            this.isCancelled = false;
            this.localFileName = String.Empty;

            this.DownloadStartTime = DateTime.Now;

            this.attemptNumber = 0;

            StartDownload();
        }

        private void OnDownloadAttemptTimer(object sender, EventArgs eventArgs) => StartDownload();

        private void StartDownload()
        {
            if (IsCancelled())
            {
                return;
            }

            this.localFileName = ComposeLocalFilename();

            if (!this.UseCaching)
            {
                TriggerWebClientDownloadFileAsync();
                return;
            }

            this.TotalBytesToReceive = -1;
            WebHeaderCollection headers = GetHttpHeaders(this.fileSource);
            if (headers != null)
            {
                this.TotalBytesToReceive = headers.GetContentLength();
            }

            if (this.TotalBytesToReceive == -1)
            {
                this.TotalBytesToReceive = 0;
                TriggerWebClientDownloadFileAsync();
            }
            else
            {
                ResumeDownload(headers);
            }
        }

        private void ResumeDownload(WebHeaderCollection headers)
        {
            this.isFallback = false;

            string downloadedFileName = GetDestinationFileName(headers);

            if (!FileUtils.TryGetFileSize(downloadedFileName, out long downloadedFileSize))
            {
                ////todo: handle this case in future. Now in case of error we simply proceed with downloadedFileSize=0
            }

            if (this.UseCaching)
            {
                this.downloadCache.Add(this.fileSource, this.localFileName, headers);
            }

            if (downloadedFileSize > this.TotalBytesToReceive)
            {
                InvalidateCache(this.fileSource);
            }

            if (downloadedFileSize != this.TotalBytesToReceive)
            {
                if (!FileUtils.ReplaceFile(downloadedFileName, this.localFileName))
                {
                    InvalidateCache(this.fileSource);
                }

                Download(this.fileSource, this.localFileName, this.TotalBytesToReceive);
            }
            else
            {
                DownloadFromCache(downloadedFileName);
            }
        }

        private void DownloadFromCache(string cachedResource)
        {
            OnDownloadProgressChanged(this, new DownloadFileProgressChangedArgs(100, this.TotalBytesToReceive, this.TotalBytesToReceive));
            InvokeDownloadCompleted(CompletedState.Succeeded, cachedResource, null, true);
            this.readyToDownload.Set();
        }

        private void TriggerWebClientDownloadFileAsync()
        {
            try
            {
                this.isFallback = true;
                string destinationDirectory = Path.GetDirectoryName(this.localFileName);
                if (destinationDirectory != null && !Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }
                TryCleanupExistingDownloadWebClient();

                this.downloadWebClient = CreateWebClient();
                this.downloadWebClient.DownloadFileAsync(this.fileSource, this.localFileName);
            }
            catch (Exception ex)
            {
                if (!AttemptDownload())
                {
                    InvokeDownloadCompleted(CompletedState.Failed, this.localFileName, ex);
                }
            }
        }

        private DownloadWebClient CreateWebClient()
        {
            DownloadWebClient webClient = new DownloadWebClient();
            webClient.DownloadFileCompleted += OnDownloadCompleted;
            webClient.DownloadProgressChanged += OnDownloadProgressChanged;
            webClient.OpenReadCompleted += OnOpenReadCompleted;
            return webClient;
        }

        private void TryCleanupExistingDownloadWebClient()
        {
            if (this.downloadWebClient == null)
            {
                return;
            }
            try
            {
                lock (this)
                {
                    if (this.downloadWebClient != null)
                    {
                        this.downloadWebClient.DownloadFileCompleted -= OnDownloadCompleted;
                        this.downloadWebClient.DownloadProgressChanged -= OnDownloadProgressChanged;
                        this.downloadWebClient.OpenReadCompleted -= OnOpenReadCompleted;
                        this.downloadWebClient.CancelAsync();
                        this.downloadWebClient.Dispose();
                        this.downloadWebClient = null;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private bool AttemptDownload()
        {
            if (++this.attemptNumber <= this.MaxAttempts)
            {
                this.attemptTimer.Interval = this.DelayBetweenAttempts.TotalMilliseconds;
                this.attemptTimer.AutoReset = false;
                this.attemptTimer.Start();
                return true;
            }

            this.readyToDownload.Set();
            return false;
        }

        private string GetDestinationFileName(WebHeaderCollection headers)
        {
            if (!this.UseCaching)
            {
                return this.localFileName;
            }

            string cachedDestinationPath = this.downloadCache.Get(this.fileSource, headers);
            if (cachedDestinationPath == null)
            {
                //logger.Debug("No cache item found. Source: {0} Destination: {1}", fileSource, localFileName);
                DeleteDownloadedFile();
                return this.localFileName;
            }

            //logger.Debug("Download resource was found in cache. Source: {0} Destination: {1}", fileSource, cachedDestinationPath);
            return cachedDestinationPath;
        }

        private string ComposeLocalFilename()
        {
            if (this.useFileNameFromServer)
            {
                return Path.Combine(this.destinationFolder, String.Format("{0}.tmp", Guid.NewGuid()));
            }
            return Path.Combine(this.destinationFolder, this.destinationFileName);
        }

        private void Download(Uri source, string fileDestination, long totalBytesToReceive)
        {
            try
            {
                FileUtils.TryGetFileSize(fileDestination, out long seekPosition);

                TryCleanupExistingDownloadWebClient();
                this.downloadWebClient = CreateWebClient();
                this.downloadWebClient.OpenReadAsync(source, seekPosition);
            }
            catch (Exception e)
            {
                if (!AttemptDownload())
                {
                    InvokeDownloadCompleted(CompletedState.Failed, this.localFileName, e);
                }
            }
        }

        private WebHeaderCollection GetHttpHeaders(Uri source)
        {
            try
            {
                WebRequest webRequest = WebRequest.Create(source);
                webRequest.Method = WebRequestMethods.Http.Head;

                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    return webResponse.Headers;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            DownloadFileProgressChangedArgs e = new DownloadFileProgressChangedArgs(args.ProgressPercentage, args.BytesReceived, args.TotalBytesToReceive);

            OnDownloadProgressChanged(sender, e);
        }

        private void OnDownloadProgressChanged(object sender, DownloadFileProgressChangedArgs args)
        {
            if (this.BytesReceived < args.BytesReceived)
            {
                ////bytes growing? we have connection!
                this.attemptNumber = 1;
            }

            this.BytesReceived = args.BytesReceived;
            this.TotalBytesToReceive = args.TotalBytesToReceive;

            DownloadProgressChanged.SafeInvoke(sender, args);
        }

        private void InvokeDownloadCompleted(CompletedState downloadCompletedState, string fileName, System.Exception error = null, bool fromCache = false)
        {
            TimeSpan downloadTime = fromCache ? TimeSpan.Zero : DateTime.Now.Subtract(this.DownloadStartTime);
            if (this.worker != null)
            {
                this.BytesReceived = this.worker.Position;
            }

            DownloadFileCompleted.SafeInvoke(this, new DownloadFileCompletedArgs(downloadCompletedState, fileName, this.fileSource, downloadTime, this.TotalBytesToReceive, this.BytesReceived, error));
        }

        private void OnOpenReadCompleted(object sender, OpenReadCompletedEventArgs args)
        {
            DownloadWebClient webClient = sender as DownloadWebClient;
            if (webClient == null)
            {
                return;
            }

            lock (this.cancelSync)
            {
                if (this.isCancelled)
                {
                    return;
                }

                if (!webClient.HasResponse)
                {
                    TriggerWebClientDownloadFileAsync();
                    return;
                }

                bool appendExistingChunk = webClient.IsPartialResponse;
                Stream destinationStream = CreateDestinationStream(appendExistingChunk);
                if (destinationStream != null)
                {
                    TrySetStreamReadTimeout(args.Result, (int)this.SourceStreamReadTimeout.TotalMilliseconds);

                    this.worker = new StreamCopyWorker();
                    this.worker.Completed += OnWorkerCompleted;
                    this.worker.ProgressChanged += OnWorkerProgressChanged;
                    this.worker.CopyAsync(args.Result, destinationStream, this.TotalBytesToReceive);
                }
            }
        }

        private bool TrySetStreamReadTimeout(Stream stream, int timeout)
        {
            try
            {
                stream.ReadTimeout = timeout;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private Stream CreateDestinationStream(bool append)
        {
            FileStream destinationStream = null;
            try
            {
                string destinationDirectory = Path.GetDirectoryName(this.localFileName);
                if (destinationDirectory != null && !Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                destinationStream = new FileStream(this.localFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                if (append)
                {
                    destinationStream.Seek(0, SeekOrigin.End);
                }
                else
                {
                    destinationStream.SetLength(0);
                }
            }
            catch (Exception ex)
            {
                if (destinationStream != null)
                {
                    destinationStream.Dispose();
                    destinationStream = null;
                }
                OnDownloadCompleted(this.downloadWebClient, new AsyncCompletedEventArgs(ex, false, null));
            }
            return destinationStream;
        }

        private void OnWorkerProgressChanged(object sender, StreamCopyProgressEventArgs eventArgs)
        {
            if (this.isCancelled)
            {
                return;
            }

            if (this.TotalBytesToReceive == 0)
            {
                return;
            }
            long progress = eventArgs.BytesReceived / this.TotalBytesToReceive;
            int progressPercentage = (int)(progress * 100);

            OnDownloadProgressChanged(this, new DownloadFileProgressChangedArgs(progressPercentage, eventArgs.BytesReceived, this.TotalBytesToReceive));
        }

        private void OnWorkerCompleted(object sender, StreamCopyCompleteEventArgs eventArgs)
        {
            try
            {
                OnDownloadCompleted(this.downloadWebClient, new AsyncCompletedEventArgs(eventArgs.Exception, eventArgs.CompleteState == CompletedState.Canceled, null));
            }
            finally
            {
                this.worker.ProgressChanged -= OnWorkerProgressChanged;
                this.worker.Completed -= OnWorkerCompleted;
                this.worker.Dispose();
            }
        }

        /// <summary>
        /// OnDownloadCompleted event handler
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="args">AsyncCompletedEventArgs instance</param>
        protected void OnDownloadCompleted(object sender, AsyncCompletedEventArgs args)
        {
            DownloadWebClient webClient = sender as DownloadWebClient;
            if (webClient == null)
            {
                //logger.Warn("Wrong sender in OnDownloadCompleted: Actual:{0} Expected:{1}", sender.GetType(), typeof(DownloadWebClient));
                InvokeDownloadCompleted(CompletedState.Failed, this.localFileName);
                return;
            }

            if (args.Cancelled)
            {
                //logger.Debug("Download cancelled. Source: {0} Destination: {1}", fileSource, localFileName);
                DeleteDownloadedFile();

                InvokeDownloadCompleted(CompletedState.Canceled, this.localFileName);
                this.readyToDownload.Set();
            }
            else if (args.Error != null)
            {
                if (this.isFallback)
                {
                    DeleteDownloadedFile();
                }

                ////We may have NameResolutionFailure on internet connectivity problem.
                ////We don't use DnsFallbackResolver if we successfully started downloading, and then got internet problem.
                ////If we change [this.fileSource] here - we lose downloaded chunk in Cache (i.e. we create a new Cache item for new [this.fileSource]
                if (this.attemptNumber == 1 && this.DnsFallbackResolver != null && IsNameResolutionFailure(args.Error))
                {
                    Uri newFileSource = this.DnsFallbackResolver.Resolve(this.fileSource);
                    if (newFileSource != null)
                    {
                        this.fileSource = newFileSource;
                        AttemptDownload();
                        return;
                    }
                }

                if (!AttemptDownload())
                {
                    InvokeDownloadCompleted(CompletedState.Failed, null, args.Error);
                    this.readyToDownload.Set();
                }
            }
            else
            {
                if (this.useFileNameFromServer)
                {
                    this.localFileName = ApplyNewFileName(this.localFileName, webClient.GetOriginalFileNameFromDownload());
                }

                if (this.UseCaching)
                {
                    this.downloadCache.Add(this.fileSource, this.localFileName, webClient.ResponseHeaders);
                }

                ////we may have the destination file not immediately closed after downloading
                WaitFileClosed(this.localFileName, TimeSpan.FromSeconds(3));

                InvokeDownloadCompleted(CompletedState.Succeeded, this.localFileName, null);
                this.readyToDownload.Set();
            }
        }

        /// <summary>
        /// Rename oldFilePath to newFileName , placing file in same folder or in temporary folder if renaming failed. 
        /// </summary>
        /// <param name="oldFilePath">Full path and name of the file to be renamed</param>
        /// <param name="newFileName">New file name</param>
        /// <returns>Full path to renamed file</returns>
        protected virtual string ApplyNewFileName(string oldFilePath, string newFileName)
        {
            string downloadedFileName = Path.GetFileName(oldFilePath);
            string downloadDirectory = Path.GetDirectoryName(oldFilePath);

            if (newFileName == null || newFileName == downloadedFileName || downloadDirectory == null)
            {
                return oldFilePath;
            }

            string newFilePath = Path.Combine(downloadDirectory, newFileName);

            if (File.Exists(newFilePath))
            {
                try
                {
                    File.Delete(newFilePath);
                }
                catch (Exception)
                {
                    newFilePath = Path.Combine(CreateTempFolder(downloadDirectory), newFileName);
                }
            }

            if (newFilePath == oldFilePath)
            {
                return oldFilePath;
            }

            File.Move(oldFilePath, newFilePath);
            return newFilePath;
        }

        private void TriggerDownloadWebClientCancelAsync()
        {
            if (this.downloadWebClient != null)
            {
                this.downloadWebClient.CancelAsync();
                this.downloadWebClient.OpenReadCompleted -= OnOpenReadCompleted;
            }
        }

        private string CreateTempFolder(string rootFolderPath)
        {
            while (true)
            {
                string folderPath = Path.Combine(rootFolderPath, Path.GetRandomFileName());
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    return folderPath;
                }
            }
        }

        private bool IsNameResolutionFailure(System.Exception exception)
        {
            WebException webException = exception as WebException;
            return webException != null && webException.Status == WebExceptionStatus.NameResolutionFailure;
        }

        private bool WaitSafeStart()
        {
            if (!this.readyToDownload.WaitOne(this.SafeWaitTimeout))
            {
                return false;
            }
            this.readyToDownload.Reset();
            return true;
        }

        private void WaitFileClosed(string fileName, TimeSpan waitTimeout)
        {
            TimeSpan waitCounter = TimeSpan.Zero;
            while (waitCounter < waitTimeout)
            {
                try
                {
                    FileStream fileHandle = File.Open(fileName, FileMode.Open, FileAccess.Read);
                    fileHandle.Dispose();
                    Thread.Sleep(500);
                    return;
                }
                catch (System.Exception)
                {
                    waitCounter = waitCounter.Add(TimeSpan.FromMilliseconds(500));
                    Thread.Sleep(500);
                }
            }
        }

        private bool IsCancelled()
        {
            lock (this.cancelSync)
            {
                if (this.isCancelled)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Do the actual dispose
        /// </summary>
        /// <param name="disposing">True if called from Dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.readyToDownload.WaitOne(TimeSpan.FromMinutes(10)))
                    {
                        if (this.worker != null)
                        {
                            this.worker.Dispose();
                        }
                        if (this.downloadWebClient != null)
                        {
                            this.downloadWebClient.Dispose();
                        }
                        this.readyToDownload.Close();
                        this.attemptTimer.Dispose();
                    }
                }
                this.disposed = true;
            }
        }
    }
}