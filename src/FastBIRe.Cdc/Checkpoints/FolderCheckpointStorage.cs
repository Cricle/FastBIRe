using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.Checkpoints
{
    public class FolderCheckpointStorage : ICheckpointStorage
    {
        public FolderCheckpointStorage(string folder)
        {
            Folder = folder ?? throw new ArgumentNullException(nameof(folder));
        }

        public string Folder { get; }

        public bool FolderExists => Directory.Exists(Folder);

        private void EnsureFolderCreated()
        {
            if (!FolderExists)
            {
                new DirectoryInfo(Folder).Create();
            }
        }
        private void EnumerableFiles(Action<string> action,Action<string>? folderComplated=null)
        {
            foreach (var item in Directory.EnumerateDirectories(Folder))
            {
                foreach (var fi in Directory.EnumerateFiles(item))
                {
                    action(fi);
                }
                folderComplated?.Invoke(item);
            }
        }
        public Task<int?> CleanAsync(CancellationToken token = default)
        {
            var c = 0;
            EnsureFolderCreated();
            EnumerableFiles(fi =>
            {
                File.Delete(fi);
                c++;
            }, s => Directory.Delete(s, true));
            return Task.FromResult<int?>(c);
        }

        public Task<int> CountAsync(string? databaseName, CancellationToken token = default)
        {
            EnsureFolderCreated();
            var count = Directory.EnumerateFiles(Folder, "*", SearchOption.AllDirectories).Count();
            return Task.FromResult(count);
        }
        private CheckpointIdentity CreateIdentity(string path)
        {
            var databaseName = Path.GetFileName(Path.GetDirectoryName(path));
            var tableName = Path.GetFileName(path);
            return new CheckpointIdentity(databaseName, tableName);
        }
        private CheckpointPackage CreatePackage(string path)
        {
            var datas = File.ReadAllBytes(path);
            return new CheckpointPackage(CreateIdentity(path), datas);
        }
        public Task<IList<CheckpointPackage>> GetAllAsync(CancellationToken token = default)
        {
            EnsureFolderCreated();
            var res = new List<CheckpointPackage>();
            EnumerableFiles(fi => res.Add(CreatePackage(fi)));
            return Task.FromResult<IList<CheckpointPackage>>(res);
        }

        public Task<CheckpointPackage?> GetAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            var fi = Path.Combine(Folder, databaseName, tableName);
            if (File.Exists(fi))
            {
                return Task.FromResult<CheckpointPackage?>(CreatePackage(fi));
            }
            return Task.FromResult<CheckpointPackage?>(null);
        }

        public Task<IList<CheckpointPackage>> GetAsync(string databaseName, CancellationToken token = default)
        {
            var res= new List<CheckpointPackage>(0);
            var path = Path.Combine(Folder, databaseName);
            if (!Directory.Exists(path))
            {
                return Task.FromResult<IList<CheckpointPackage>>(res);
            }
            foreach (var item in Directory.EnumerateFiles(path))
            {
                res.Add(CreatePackage(item));
            }
            return Task.FromResult<IList<CheckpointPackage>>(res);
        }
        private bool Remove(CheckpointPackage package)
        {
            var fi = Path.Combine(Folder, package.Identity.DatabaseName, package.Identity.TableName);
            if (File.Exists(fi))
            {
                File.Delete(fi);
                return true;
            }
            return false;
        }
        public Task<bool> RemoveAsync(CheckpointPackage package, CancellationToken token = default)
        {
            return Task.FromResult(Remove(package));
        }

        public Task<int> RemoveAsync(IEnumerable<CheckpointPackage> packages, CancellationToken token = default)
        {
            var c = 0;
            foreach (var item in packages)
            {
                c += Remove(item) ? 1 : 0;
            }
            return Task.FromResult(c);
        }
        private void Write(CheckpointPackage package)
        {
            var dir = new DirectoryInfo(Path.Combine(Folder, package.Identity.DatabaseName));
            if (!dir.Exists)
            {
                dir.Create();
            }
            var fi = Path.Combine(dir.FullName, package.Identity.TableName);
            File.WriteAllBytes(fi, package.CheckpointData ?? Array.Empty<byte>());
        }
        public Task<bool> SetAsync(CheckpointPackage package, CancellationToken token = default)
        {
            Write(package);
            return Task.FromResult(true);
        }

        public Task<int> SetAsync(IEnumerable<CheckpointPackage> packages, CancellationToken token = default)
        {
            var c = 0;
            foreach (var item in packages)
            {
                Write(item);
                c++;
            }
            return Task.FromResult(c);
        }
    }
}
