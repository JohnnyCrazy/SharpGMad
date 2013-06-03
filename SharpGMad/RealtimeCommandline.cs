﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGMad
{
    class Realtime
    {
        static Addon addon;
        static string filePath;
        static string CommandlinePrefix = "SharpGMad>";

        public static int Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Realtime command-line activated.");
            Console.ResetColor();

            // There might be a parameter specified.
            string strFile;
            try
            {
                int fileIndex = Array.FindIndex(args, a => a == "-file");

                if (fileIndex == -1)
                    throw new Exception(); // This means that the switch does not exist
                else
                    strFile = args[Array.FindIndex(args, a => a == "-file") + 1];
            }
            catch (Exception)
            {
                strFile = "";
            }

            // Autoload addon from command line arguments
            if (strFile != String.Empty && strFile != null)
                LoadAddon(strFile);

            while (true)
            {
                Console.Write(CommandlinePrefix + " ");
                string input = Console.ReadLine();
                string[] command = input.Split(' ');

                switch (command[0].ToLowerInvariant())
                {
                    case "new":
                        try
                        {
                            NewAddon(command[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The filename was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        break;
                    case "load":
                        try
                        {
                            LoadAddon(command[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The filename was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        break;
                    case "add":
                        try
                        {
                            AddFile(command[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The file path was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        break;
                    case "addfolder":
                        try
                        {
                            AddFolder(command[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The folder was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        break;
                    case "list":
                        ListFiles();
                        break;
                    case "remove":
                        try
                        {
                            RemoveFile(command[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The filename was not specified.");
                            Console.ResetColor();
                            break;
                        }

                        break;
                    case "get":
                        string parameter;
                        try
                        {
                            parameter = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The parameter was not specified.");
                            Console.ResetColor();
                            Console.WriteLine("The valid paramteres are: author description tags title type");
                            break;
                        }

                        switch (parameter)
                        {
                            case "author":
                                Console.WriteLine(addon.Author);
                                break;
                            case "description":
                                Console.WriteLine(addon.Description);
                                break;
                            case "tags":
                                Console.WriteLine(String.Join(" ", addon.Tags.ToArray()));
                                break;
                            case "title":
                                Console.WriteLine(addon.Title);
                                break;
                            case "type":
                                Console.WriteLine(addon.Type);
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine("The specified parameter is not valid.");
                                Console.ResetColor();
                                Console.WriteLine("The valid paramteres are: author description tags title type");
                                break;
                        }

                        break;
                    case "set":
                        string Sparameter;
                        try
                        {
                            Sparameter = args[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The parameter was not specified.");
                            Console.ResetColor();
                            Console.WriteLine("The valid paramteres are: author description tags title type");
                            break;
                        }

                        string value;
                        try
                        {
                            string[] param = new string[args.Length - 2];
                            for (int i = 2; i < args.Length; i++)
                            {
                                param[i - 2] = args[i];
                            }

                            value = String.Join(" ", param);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // Noop.
                            value = String.Empty;
                        }

                        switch (Sparameter)
                        {
                            case "author":
                                SetAuthor(addon, value);
                                break;
                            case "description":
                                SetDescription(addon, value);
                                break;
                            case "tags":
                                SetTags(addon, value.Split(' '));
                                break;
                            case "title":
                                SetTitle(addon, value);
                                break;
                            case "type":
                                SetType(addon, value);
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine("The specified parameter is not valid.");
                                Console.ResetColor();
                                Console.WriteLine("The valid paramteres are: author description tags title type");
                                break;
                        }

                        break;
                    case "close":
                        CloseAddon();
                        break;
                    case "push":
                        Push();
                        break;
                    case "path":
                        FullPath();
                        break;
                    case "pwd":
                        Console.WriteLine(Directory.GetCurrentDirectory());
                        break;
                    case "cd":
                        try
                        {
                            Directory.SetCurrentDirectory(command[1]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The folder was not specified.");
                            Console.ResetColor();
                            break;
                        }
                        catch (IOException e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("There was a problem switching to that folder.");
                            Console.ResetColor();
                            Console.WriteLine(e.Message);
                            break;
                        }
                        break;
                    case "ls":
                        try
                        {
                            IEnumerable<string> files = Directory.EnumerateFiles(Directory.GetCurrentDirectory(),
                                "*", SearchOption.TopDirectoryOnly);

                            if (files.Count() == 0)
                            {
                                Console.WriteLine("0 files.");
                                break;
                            }
                            else
                                Console.WriteLine(files.Count() + " files:");

                            foreach (string f in files)
                            {
                                FileInfo fi = new FileInfo(f);

                                Console.WriteLine(
                                    String.Format("{0,10} {1,20} {2,30}", ((int)fi.Length).HumanReadableSize(),
                                    fi.LastWriteTime.ToString(), fi.Name)
                                );
                            }
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("There was a problem listing the files.");
                            Console.ResetColor();
                            Console.WriteLine(e.Message);
                            break;
                        }

                        break;
                    case "gui":
                        System.Windows.Forms.Application.Run(new Main(args));
                        break;
                    case "?":
                    case "help":
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Available commands:");
                        Console.ResetColor();

                        if (addon == null)
                        {
                            Console.WriteLine("load <filename>            Loads <filename> addon into the memory");
                            Console.WriteLine("new <filename>             Create a new, empty addon named <filename>");
                        }

                        if (addon is Addon)
                        {
                            Console.WriteLine("add <filename>             Adds <filename> to the archive");
                            Console.WriteLine("addfolder <folder>         Adds all files from <folder> to the archive");
                            Console.WriteLine("list                       Lists the files in the memory");
                            Console.WriteLine("remove <filename>          Removes <filename> from the archive");
                            Console.WriteLine("get <parameter>            Prints the value of metadata <parameter>");
                            Console.WriteLine("set <parameter> [value]    Sets metadata <parameter> to the specified [value]");
                            Console.WriteLine("push                       Writes the changes to the disk");
                            Console.WriteLine("close                      Closes the addon (dropping all changes)");
                            Console.WriteLine("path                       Prints the full path of the current addon.");
                        }

                        Console.WriteLine("pwd                        Prints SharpGMad's current working directory");
                        Console.WriteLine("cd <folder>                Changes the current working directory to <folder>");
                        Console.WriteLine("ls                         List all files in the current directory");
                        Console.WriteLine("gui                        Load the GUI");
                        Console.WriteLine("help                       Show the list of available commands");

                        if (addon == null)
                        {
                            Console.WriteLine("exit                       Exits");
                        }

                        break;
                    case "exit":
                        if (addon is Addon)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Cannot exit. An addon is open!");
                            Console.ResetColor();
                            break;
                        }

                        return 0;
                        //break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unknown operation.");
                        Console.ResetColor();
                        break;
                }
            }
        }

        static void NewAddon(string filename)
        {
            if (addon is Addon)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An addon is already open. Please close it first.");
                Console.ResetColor();
                return;
            }

            if (File.Exists(filename))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("This file already exists. You might want to load it instead.");
                Console.ResetColor();
                return;
            }

            //
            // Make sure OutFile ends in .gma
            //
            filename = Path.GetFileNameWithoutExtension(filename);
            filename += ".gma";

            Console.WriteLine("Creating new file...");

            CommandlinePrefix = filename + " (setup)>";
            addon = new Addon();

            SetTitle(addon);
            SetDescription(addon);
            //SetAuthor(addon); // Author name setting is not yet implemented.
            SetType(addon);
            SetTags(addon);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Successfully set up initial addon.");
            Console.ResetColor();

            // Write initial content
            filePath = Path.GetFullPath(filename);
            FileStream fileStream;
            try
            {
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was a problem opening the file.");
                Console.ResetColor();
                Console.WriteLine(e.Message);
                CloseAddon();
                return;
            }

            MemoryStream ms;
            StreamDiffer sd = new StreamDiffer(fileStream);
            Writer.Create(addon, out ms);
            sd.Write(ms);
            sd.Push();

            fileStream.Close();

            CommandlinePrefix = Path.GetFileName(filename) + ">";
        }

        private static void SetTitle(Addon addon, string title = null)
        {
            if (title == String.Empty || title == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Title? ");
                Console.ResetColor();
                title = Console.ReadLine();
            }

            addon.Title = title;
        }

        private static void SetDescription(Addon addon, string description = null)
        {
            if (description == String.Empty || description == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Short description? ");
                Console.ResetColor();
                description = Console.ReadLine();
            }

            addon.Description = description;
        }

        private static void SetAuthor(Addon addon, string author = null)
        {
            if (author == String.Empty || author == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Author? ");
                Console.ResetColor();
                Console.WriteLine("This currently has no effect as the addon is always written with \"Author Name\".");
                //author = Console.ReadLine();
                author = "Author Name";
            }

            addon.Author = author;
        }

        private static void SetType(Addon addon, string type = null)
        {
            if (type == String.Empty || type == null)
            {
                while (!Tags.TypeExists(type))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Type? ");
                    Console.ResetColor();
                    Console.Write("Please choose ONE from the following: ");
                    Console.WriteLine(String.Join(" ", Tags.Type));
                    type = Console.ReadLine();

                    if (!Tags.TypeExists(type))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("The specified type is not valid.");
                        Console.ResetColor();
                    }
                }
            }
            else
            {
                if (!Tags.TypeExists(type))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("The specified type is not valid.");
                    Console.ResetColor();
                    return;
                }
            }

            addon.Type = type;
        }

        private static void SetTags(Addon addon, string[] tagsInput = null)
        {
            List<string> tags = new List<string>(2);
            if (tagsInput == null || tagsInput.Length == 0 || tagsInput[0] == String.Empty)
            {
                bool allTagsValid = false;
                while (!allTagsValid)
                {
                    tags.Clear();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Tags? ");
                    Console.ResetColor();
                    Console.Write("Please choose ZERO, ONE or TWO from the following: ");
                    Console.WriteLine(String.Join(" ", Tags.Misc));

                    tagsInput = Console.ReadLine().Split(' ');

                    allTagsValid = true;
                    if (tagsInput[0] != String.Empty)
                    {
                        // More than zero (one or two) elements: add the first one.
                        if (tagsInput.Length > 0)
                        {
                            if (!Tags.TagExists(tagsInput[0]))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("The specified tag \"" + tagsInput[0] + "\" is not valid.");
                                Console.ResetColor();
                                allTagsValid = false;
                                continue;
                            }
                            else
                                tags.Add(tagsInput[0]);
                        }

                        // More than one (two) elements: add the second one too.
                        if (tagsInput.Length > 1)
                        {
                            if (!Tags.TagExists(tagsInput[1]))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("The specified tag \"" + tagsInput[1] + "\" is not valid.");
                                Console.ResetColor();
                                allTagsValid = false;
                                continue;
                            }
                            else
                                tags.Add(tagsInput[1]);
                        }

                        if (tagsInput.Length > 2)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("More than two tags specified. Only the first two is saved.");
                            Console.ResetColor();
                        }
                    }
                }
            }
            else
            {
                if (tagsInput[0] != String.Empty)
                {
                    // More than zero (one or two) elements: add the first one.
                    if (tagsInput.Length > 0)
                    {
                        if (!Tags.TagExists(tagsInput[0]))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The specified tag \"" + tagsInput[0] + "\" is not valid.");
                            Console.ResetColor();
                            return;
                        }
                        else
                            tags.Add(tagsInput[0]);
                    }

                    // More than one (two) elements: add the second one too.
                    if (tagsInput.Length > 1)
                    {
                        if (!Tags.TagExists(tagsInput[1]))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The specified tag \"" + tagsInput[1] + "\" is not valid.");
                            Console.ResetColor();
                            return;
                        }
                        else
                            tags.Add(tagsInput[1]);
                    }

                    if (tagsInput.Length > 2)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("More than two tags specified. Only the first two is saved.");
                        Console.ResetColor();
                    }
                }
            }

            addon.Tags = tags;
        }

        static void LoadAddon(string filename)
        {
            if (addon is Addon)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An addon is already open. Please close it first.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("Loading file...");
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
            addon = new Addon(new Reader(fs));

            foreach (ContentFile f in addon.Files)
                Console.WriteLine("\t" + f.Path + " [" + ((int)f.Size).HumanReadableSize() + "]");

            filePath = Path.GetFullPath(filename);
            CommandlinePrefix = Path.GetFileName(filename) + ">";
        }

        static void AddFile(string filename)
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            byte[] bytes;
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, (int)fs.Length);
                }
            }
            catch (IOException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Cannot add file. There was an error reading it.");
                Console.ResetColor();
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine(filename + " as");
            Console.WriteLine("\t" + Whitelist.GetMatchingString(filename));
            try
            {
                addon.AddFile(Whitelist.GetMatchingString(filename), bytes);
            }
            catch (IgnoredException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\t\t[Ignored]");
                Console.ResetColor();
                return;
            }
            catch (WhitelistException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\t\t[Not allowed by whitelist]");
                Console.ResetColor();
                return;
            }
            catch (ArgumentException e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\t\t[A file like this has already been added. Remove it first.]");
                Console.ResetColor();
                return;
            }
        }

        static void AddFolder(string folder)
        {
            foreach (string f in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
            {
                string file = f;
                file = file.Replace('\\', '/');

                AddFile(file);
            }
        }

        static void ListFiles()
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine(addon.Files.Count + " files in archive:");
            foreach (ContentFile file in addon.Files)
            {
                Console.WriteLine(file.Path + " (" + ((int)file.Size).HumanReadableSize() + ")");
            }
        }

        static void RemoveFile(string filename)
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            try
            {
                addon.RemoveFile(filename);
            }
            catch (FileNotFoundException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The file was not found in the archive.");
                Console.ResetColor();
                return;
            }
        }

        static void Push()
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            FileStream fileStream;
            try
            {
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was a problem opening the file.");
                Console.ResetColor();
                Console.WriteLine(e.Message);
                return;
            }

            MemoryStream ms;
            StreamDiffer sd = new StreamDiffer(fileStream);
            Writer.Create(addon, out ms);
            sd.Write(ms);
            int count = sd.Push();

            fileStream.Close();

            Console.WriteLine("Successfully saved. " + count.HumanReadableSize() + " was modified.");
        }

        static void CloseAddon()
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            addon = null;
            CommandlinePrefix = "SharpGMad>";
        }

        static void FullPath()
        {
            if (addon == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No addon is open.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine(Path.GetFullPath(filePath));
        }
    }
}