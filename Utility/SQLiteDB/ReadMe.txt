Note on using SQLiteDB helper:

SQLite relies on dynamically loaded correct SQLite.Interop.dll, which are in x64 and x86 directories.
These will be automatically copied if this library is referrenced in another project.
However, if that another project is referrenced elsewhere, these may not be copied. 
This can be simply fixed by doing the copy manually and linking files as content.