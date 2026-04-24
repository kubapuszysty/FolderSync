# FolderSync
Simple console application written in C# that keeps a replica folder in sync with a source folder. It runs at a given internal, copying new and updated files, removing anything that no longer exists in the source, and preserving the full directory structure (including empty folders). File changes are detected using MD5 hashes, and all operations are logged to both the console and file.
## Usage
Run the application from the command line:
```C#
dotnet run "C:\source" "C:\replica" 10 "C:\log.txt"
```
