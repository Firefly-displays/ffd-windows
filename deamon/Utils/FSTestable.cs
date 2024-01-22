using System.IO;
using System.IO.Abstractions;

namespace deamon.Utils;

public abstract class FSTestable
{
    protected readonly IFileSystem FS;

    protected FSTestable() : this (new FileSystem()) {}

    protected FSTestable(IFileSystem fs)
    {
        FS = fs;
    }
}