using MessagePipe;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathManager
{
    public enum RequestType
    {
        Get,
        Set,
        Undo,
        Redo
    }

    public struct PathRequest
    {
        public RequestType RequestType;
        public string Path;
    }

    public struct PathResponse
    {
        public string Path;
    }

    public class PathManager : BackgroundService, IRequestHandler<PathRequest, PathResponse>
    {
        private DirectoryInfo? _currentPath;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _currentPath = new DirectoryInfo(@"C:\projects\Example");
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public PathResponse Invoke(PathRequest request)
        {
            var result = request.RequestType switch
            {
                RequestType.Get => new PathResponse() { Path = _currentPath.FullName },
                _ => throw new Exception()
            };
            return result;
        }
    }
}
