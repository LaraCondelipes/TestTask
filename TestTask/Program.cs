using System.Timers;

class Program
{
    //declaration of global variables
    private static System.Timers.Timer sync;
    private static string origin;
    private static string replica;
    private static string logPath;
    private static int interval;
    static void Main(string[] args)
    {
        //validates that they are sending the right number of arguments
        if (args.Length !=4) 
        {
            Console.WriteLine("Invalid Arguments");
            Console.WriteLine("Arguments Needed:");
            Console.WriteLine("1- Origin path");
            Console.WriteLine("2- Replica path");
            Console.WriteLine("3- Log path");
            Console.WriteLine("4- Sync interval");
            Console.ReadLine();
            return;
        }

        origin = args[0];
        replica = args[1];
        logPath = args[2];
        interval = int.Parse(args[3]);
        var proceed = false;

        //validates if the folders exist 

        if (!HandleDirectoryParameters(origin, "Origin Path"))
        {
            return;
        }

        if (!HandleDirectoryParameters(replica, "Replica Path"))
        {
            return;
        }

        if (!HandleDirectoryParameters(logPath, "Log Path"))
        {
            return;
        }

        //timer for synchronisation
        sync = new System.Timers.Timer(interval);
        sync.Elapsed += Sync_Elapsed; 
        //enable tem de ser true para o timer ser executado
        sync.Enabled = true;

        Console.ReadLine();
    }

    private static void Sync_Elapsed(object? sender, ElapsedEventArgs e)
    {
        //block of code that we want the timer to execute
        //get list of files in origin folder
        List<string> list = new List<string>(Directory.EnumerateFiles(origin));
        foreach (string dir in list)
        {
            FileInfo fileInfo = new FileInfo(dir);
            //Mostra o nome do arquivo
            string fileName = fileInfo.Name;
            string destination = replica + "\\" + fileName;


            if (IsFileDifferent(fileName))
            {
                //copy file from origin to replica
                File.Copy(dir, destination, true);

                Console.WriteLine($"{DateTime.Now} - File {dir} copied to {destination}");
                //logar para ficheiro
                using (StreamWriter sw = File.AppendText(logPath + "\\log.txt"))
                {
                    sw.WriteLine($"{DateTime.Now} - File {dir} copied to {destination}");
                }
            }
        }

        DeleteReplicaFilesNotPresentInOrigin(origin, replica);
    }

    //to copy only the modified files
    private static bool IsFileDifferent(string fileName)
    {
        FileInfo originFile = new FileInfo(origin + "\\" + fileName);
        FileInfo replicaFile = new FileInfo(replica + "\\" + fileName);

        return originFile.LastWriteTime > replicaFile.LastWriteTime;
    }

    private static void DeleteReplicaFilesNotPresentInOrigin(string origin, string replica)
    {
        //compare files between origin and replica
        List<string> originList = new List<string>(Directory.EnumerateFiles(origin));
        List<string> replicaList = new List<string>(Directory.EnumerateFiles(replica));

        //we need the file names to compare
        List<string> originFileNameList = new List<string>();
        List<string> replicaFileNameList = new List<string>();

        //adiciona os nomes dos ficheiros da origem a uma lista
        foreach (string dir in originList) 
        {
            FileInfo fileInfo = new FileInfo(dir);
            originFileNameList.Add(fileInfo.Name);
            
        }
        //add file names from origin to a list
        foreach (string dir in replicaList)
        {
            FileInfo fileInfo = new FileInfo(dir);
            replicaFileNameList.Add(fileInfo.Name);
        }
        //gets the file names that exist in the replica but not exist in the origin (which be deleted)
        List<string> filesToDelete = replicaFileNameList.Except(originFileNameList).ToList();

        foreach (string file in filesToDelete) 
        {
            //delete files that don't exist in the origin from the replica
            File.Delete(replica + "\\" + file);
            //log for deletelog file
            Console.WriteLine($"{DateTime.Now} - File {replica + "\\" + file} deleted.");
           
            //logar to file
            using (StreamWriter sw = File.AppendText(logPath + "\\log.txt"))
            {
                sw.WriteLine($"{DateTime.Now} - File {replica + "\\" + file} deleted.");
            }
        } 
    }

    //Methods
    private static bool HandleDirectoryParameters(string directory, string parameter)
    {
        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"The {parameter} does not exist, do you want to create it?(y/n)");

            if (Console.ReadLine() == "y")
            {
                //create folders
                Directory.CreateDirectory(directory);
            }
            else
            {
                Console.WriteLine($"Invalid parameter: {parameter}.");
                //log to file
                using (StreamWriter sw = File.AppendText(logPath + "\\log.txt"))
                {
                    sw.WriteLine($"Invalid parameter: {parameter}.");
                }
                return false;
            }
        }
        return true;
    }
}

