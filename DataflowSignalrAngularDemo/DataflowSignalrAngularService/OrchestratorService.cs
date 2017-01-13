namespace DataflowFileService
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Net;
    using System.Security.Permissions;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    using ApiSignalrAngularModel;

    using Newtonsoft.Json;

    /// <summary>
    /// </summary>
    public class OrchestratorService : IOrchestratorService
    {
        private BufferBlock<FileOrderEntity> _filesBufferBlock;
        private readonly BufferBlock<string> _bufferBlock = new BufferBlock<string>();
        private TransformManyBlock<string, string> _receptorBlockOne;
        private TransformManyBlock<string, string> _receptorBlockTwo;
        private TransformManyBlock<string, string> _receptorBlockThee;
        private ActionBlock<FileOrderEntity> _printingBlock;

       

        /// <summary>
        /// </summary>
        private TransformBlock<string, FileOrderEntity> _transformBlockFileToFileOrderEntity;

        /// <summary>
        /// </summary>
        private readonly ExecutionDataflowBlockOptions _blockConfiguration = new ExecutionDataflowBlockOptions()
        {
            NameFormat = "Type:{0},Id:{1}",
            MaxDegreeOfParallelism = 4,
        };

        // ReSharper disable once PrivateMembersMustHaveComments
        /// <summary>
        /// </summary>
        public void Execute()
        {
            this.Start();
            this.BuildFileReceptionWorkflow();
            this.ExtractOrder();
        }

        private void Start()
        {
            this._transformBlockFileToFileOrderEntity = new TransformBlock<string, FileOrderEntity>(x => this.ProcessTransformFileToFileOrderEntity(x), this._blockConfiguration);

            this._transformBlockFileToFileOrderEntity
               .Completion
               .ContinueWith(
                   dbt =>
                   {
                       if (dbt.Exception != null)
                           foreach (Exception error in dbt.Exception.Flatten().InnerExceptions)
                           {
                               Console.WriteLine("_transformBlockAutorizeVendor block failed Reason:{0}", error.Message);
                           }
                   },
                   TaskContinuationOptions.OnlyOnFaulted);

            this._printingBlock = new ActionBlock<FileOrderEntity>(x => Console.WriteLine(string.Format(" Printing {0}", x.FileName)));
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private Task<FileOrderEntity> ProcessTransformFileToFileOrderEntity(string x)
        {
            var fileOrderEntity = new FileOrderEntity { AccountNumber = x, FileName = x, Extension = ".csv" };
            Console.WriteLine("ProcessTransformFileToFileOrderEntity {0}", x);
            this.PostData(fileOrderEntity, "FileOrderEntity");
            return Task.Run(() => fileOrderEntity);
        }

        /// <summary>
        /// </summary>
        private async void ExtractOrder()
        {
            await this.BroadCastData();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private async Task BroadCastData()
        {
            await Task.Run(() => this.WatcherDirectory(@"D:\Samples\dumpdir"));
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void WatcherDirectory(string fileDirectory)
        {
            var watcher = new FileSystemWatcher();
            watcher.Path = fileDirectory;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.csv";
            watcher.Changed += this.OnChanged;
            watcher.Created += this.OnCreated;
            watcher.Deleted += this.OnChanged;
            watcher.Renamed += this.OnRenamed;
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            this._bufferBlock.Post(e.FullPath);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
        }

        /// <summary>
        ///
        /// </summary>
        public void BuildFileReceptionWorkflow()
        {
            var nonGreedy = new ExecutionDataflowBlockOptions() { BoundedCapacity = 1 };
            var flowComplete = new DataflowLinkOptions() { PropagateCompletion = true };

            this._filesBufferBlock = new BufferBlock<FileOrderEntity>();
            this._receptorBlockTwo = new TransformManyBlock<string, string>(i => this.FindFilesInDirectory("ReceiverA", i), nonGreedy);
            this._receptorBlockThee = new TransformManyBlock<string, string>(i => this.FindFilesInDirectory("ReceiverB", i), nonGreedy);
            this._receptorBlockOne = new TransformManyBlock<string, string>(i => this.FindFilesInDirectory("ReceiverC", i), nonGreedy);

            this._bufferBlock.LinkTo(this._receptorBlockOne, flowComplete);
            this._bufferBlock.LinkTo(this._receptorBlockTwo, flowComplete);
            this._bufferBlock.LinkTo(this._receptorBlockThee, flowComplete);

            this._receptorBlockOne.LinkTo(this._transformBlockFileToFileOrderEntity);
            this._receptorBlockTwo.LinkTo(this._transformBlockFileToFileOrderEntity);
            this._receptorBlockThee.LinkTo(this._transformBlockFileToFileOrderEntity);
            this._transformBlockFileToFileOrderEntity.LinkTo(this._filesBufferBlock);
            this._filesBufferBlock.LinkTo(this._printingBlock);
            this._transformBlockFileToFileOrderEntity.Completion.ContinueWith(t => this._filesBufferBlock.Complete());
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private IEnumerable<string> FindFilesInDirectory(string name, string value)
        {
            Console.WriteLine("Processor {0}, starting : {1}", name, value);
            this.PostData(new Processor { Name = name, Value = value }, "Processor");
            return new List<string> { value };
        }

        ///   <summary>
        ///
        ///   </summary>
        ///  <param name="processor"></param>
        /// <param name="routePrefix"></param>
        private void PostData<T>(T processor, string routePrefix)
        {
            var json = JsonConvert.SerializeObject(processor);
            var apiPath = ConfigurationManager.AppSettings["ServerUrl"];
            // Post the data to the server
            var serverUrl = new Uri(Path.Combine(apiPath, routePrefix));

            var client = new WebClient();
            client.Headers.Add("Content-Type", "application/json;charset=utf-8");
            Task.Run(() => client.UploadString(serverUrl, json));
        }
    }
}