using System;
using System.Collections;
using System.IO;

namespace ClothoLibStandard
{
    /// <summary>
    /// Text File IO
    /// </summary>
    public class IO_TextFile
    {
        StreamWriter writer = null;
        ArrayList tempString = new ArrayList();
        DirectoryInfo currDir = new DirectoryInfo(".");

        ~IO_TextFile() { }
        // Searching Current Directory
        /// <summary>
        /// Re-new the existing file (re-create)
        /// </summary>
        /// <param name="fileName">"File Name.txt"</param>
        public void CreateFileInDirectory(string dirpath)
        {
            try
            {
                writer = File.CreateText(@dirpath);
            }
            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException("Cannot Create file! " + e.Message);
            }
            finally
            {
                writer.Close();
            }
        }

        /// <summary>
        /// Overwrite existing Text File
        /// </summary>
        /// <param name="dirpath">E.g. C:\CSharpLearner\Day1</param>
        /// <param name="contain">ArrayList Format</param>
        public void CreateWriteToTextFile(string dirpath, ArrayList contain)
        {
            try
            {
                if (!File.Exists(@dirpath))
                    throw new FileNotFoundException("{0} does not exist."
                        , @dirpath);
                else
                {
                    using (StreamWriter writer = File.CreateText(@dirpath))
                    {
                        for (int i = 0; i < contain.Count; i++)
                        {
                            writer.WriteLine(contain[i]);
                        }
                        writer.Close();
                    }
                }
            }
            catch (FileNotFoundException)
            {

                throw new FileNotFoundException("Cannot Write Existing file!");
            }
        }
        /// <summary>
        /// Log New Data and append with new line into Existing File
        /// </summary>
        /// <param name="dirpath">E.g. C:\CSharpLearner\Day1</param>
        /// <param name="contain">E.g. new string[] { "New", "New File" }</param>
        public void WriteNewLineToExistTextFile(string dirpath, string[] contain)
        {
            try
            {
                if (!File.Exists(@dirpath))
                    throw new FileNotFoundException("{0} does not exist."
                        , @dirpath);
                else
                {
                    using (StreamWriter writer = File.AppendText(@dirpath))
                    {
                        for (int i = 0; i < contain.Length; i++)
                        {
                            writer.Write(contain[i] + ",");
                        }
                        writer.WriteLine();
                        writer.Close();
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException("Cannot Write/Append the file!\n" + ex.Message);
            }
        }
        /// <summary>
        /// Log New Data and append with new line into Existing File
        /// </summary>
        /// <param name="dirpath">E.g. C:\CSharpLearner\Day1</param>
        /// <param name="contain">contain in string format</param>
        public void WriteNewLineToExistTextFile(string dirpath, string contain)
        {
            try
            {
                if (!File.Exists(@dirpath))
                    throw new FileNotFoundException("{0} does not exist."
                        , @dirpath);
                else
                {
                    using (StreamWriter writer = File.AppendText(@dirpath))
                    {                        
                        writer.Write(contain);
                        writer.WriteLine();
                        writer.Close();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException("Cannot Write/Append the file!");
            }
        }
        /// <summary>
        /// Upgrate Data of Esxiting text File
        /// </summary>
        /// <param name="dirpath">"Setup.txt"</param>
        /// <param name="groupName">E.g. Global, Spec which is match with existing text file</param>
        /// <param name="targetUpgradeName">E.g. max, min which is match with existing text file contain</param>
        /// <param name="upgradeData">String Format, which is going to upgrate the data in existing text file</param>
        public void WriteUpgradeDataToExistTestFile(string dirpath, string groupName,
            string targetUpgradeName, string upgradeData)
        {
            ArrayList LocalTextList = new ArrayList();
            ArrayList TempTextList = new ArrayList();

            LocalTextList = ReadTextFile(dirpath);
            for (int i = 0; i < LocalTextList.Count; i++)
            {
                if (LocalTextList[i].ToString() == "[" + groupName + "]")
                {

                    char[] temp = { };
                    string[] templine;
                    i++;
                    try
                    {
                        while (LocalTextList[i].ToString() != null)
                        {
                            temp = LocalTextList[i].ToString().ToCharArray();
                            templine = LocalTextList[i].ToString()
                                .Split(new char[] { ' ', '=', ' ' });

                            if (temp[0] == '[' && temp[temp.Length - 1] == ']')
                                break;

                            if (templine[0] == targetUpgradeName)
                            {
                                templine[templine.Length - 1] = " = " + upgradeData;
                                LocalTextList[i] = string.Join("", templine);
                                break;
                            }
                            i++;
                        }
                        break;
                    }
                    catch (FileNotFoundException)
                    {

                        throw new FileNotFoundException("Cannot Find " + targetUpgradeName);
                    }
                }
            }

            CreateWriteToTextFile(dirpath, LocalTextList);
        }

        /// <summary>
        /// Read all dtaa from text file, Note: Return as ArrayList Format
        /// </summary>
        /// <param name="dirpath">"Setup.txt"</param>
        /// <returns></returns>
        public ArrayList ReadTextFile(string dirpath)
        {
            try
            {
                if (!File.Exists(@dirpath))
                {
                    throw new FileNotFoundException("{0} does not exist."
                        , @dirpath);
                }
                else
                {
                    using (StreamReader reader = File.OpenText(@dirpath))
                    {
                        string line = "";
                        tempString.Clear();

                        while ((line = reader.ReadLine()) != null)
                        {
                            tempString.Add(line.ToString());
                        }
                        reader.Close();
                    }
                }
                return tempString;
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(dirpath + " Cannot Read from the file!");
            }
        }
        /// <summary>
        /// Read section Data [ ] from Text file, Note: Return as ArrayList Format
        /// </summary>
        /// <param name="dirpath">"Setup.txt"</param>
        /// <param name="groupName">E.g. Global, Spec which is match with existing text file</param>
        /// <returns>return as ArrayList format</returns>
        public ArrayList ReadTextFile(string dirpath, string groupName)
        {
            try
            {
                if (!File.Exists(@dirpath))
                {
                    throw new FileNotFoundException("{0} does not exist."
                        , @dirpath);
                }
                else
                {
                    using (StreamReader reader = File.OpenText(@dirpath))
                    {
                        string line = "";
                        tempString.Clear();

                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line == "[" + groupName + "]")
                            {
                                char[] temp = { };
                                line = reader.ReadLine();
                                while (line != null && line != "")
                                {
                                    temp = line.ToCharArray();
                                    if (temp[0] == '[' && temp[temp.Length - 1] == ']')
                                        break;
                                    tempString.Add(line.ToString());
                                    line = reader.ReadLine();
                                }
                                break;
                            }
                        }
                        reader.Close();
                    }
                }
                return tempString;
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(dirpath + " " + groupName
                    + " Cannot Read from the file!");
            }
        }
        /// <summary>
        /// Read section Data [ ] from Text file, Note: Return as String Format
        /// </summary>
        /// <param name="dirpath">"Setup.txt"</param>
        /// <param name="groupName">E.g. Global, Spec which is match with existing text file</param>
        /// <param name="targetName">E.g. Global.NPLC, Spec.VoltageAcc.Offset</param>
        /// <returns>return as String format</returns>
        public String ReadTextFile(string dirpath, string groupName, string targetName)
        {
            string tempSingleString;
            try
            {
                if (!File.Exists(@dirpath))
                {
                    throw new FileNotFoundException("{0} does not exist."
                        , @dirpath);
                }
                else
                {
                    using (StreamReader reader = File.OpenText(@dirpath))
                    {
                        string line = "";
                        string[] templine;
                        tempSingleString = "";

                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line == "[" + groupName + "]")
                            {
                                char[] temp = { };
                                line = reader.ReadLine();
                                while (line != null && line != "")
                                {
                                    templine = line.ToString()
                                        .Split(new char[] {'='});
                                    temp = line.ToCharArray();
                                    if (temp[0] == '[' && temp[temp.Length - 1] == ']')
                                        break;
                                    if (templine[0].TrimEnd() == targetName)
                                    {
                                        tempSingleString= templine[templine.Length - 1].ToString().TrimStart();
                                        break;
                                    }
                                    line = reader.ReadLine();
                                }
                                break;
                            }
                        }
                        
                       reader.Close();
                    }
                }
                return tempSingleString;
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(dirpath + " " + groupName + " " +
                    targetName + " Cannot Read from the file!");
            }
        }

        private void RunCMD(string command)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "CMD.exe";
            startInfo.Arguments = command;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        public void CompressSDIFileInDirectory(string defaultPath, string folderName)
        {
            RunCMD("/C cd " + defaultPath + " && tar -cvf " + folderName + ".tar " + folderName);
            RunCMD("/C cd " + defaultPath + " && bzip2 " + folderName + ".tar");
        }

        public void CopyFile(string source, string target)
        {
            File.Copy(source, target);
        }
        public void DeleteFile(string source)
        {
            File.Delete(source);
        }

        public void DeleteFolder(string source)
        {
            Directory.Delete(source,true);
        }

    }
}
