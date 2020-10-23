using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace KSIM_on_B5__ver._1._0_
{
    

    public partial class Form1 : Form
    {

        //public static Regex regexQuestion;

        public partial class NativeMethods
        {
            /// Return Type: BOOL->int
            ///fBlockIt: BOOL->int
            [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "BlockInput")]
            [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
            public static extern bool BlockInput([System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)] bool fBlockIt);

            public static void BlockInput(TimeSpan span)
            {
                try
                {
                    NativeMethods.BlockInput(true);
                    Console.WriteLine("inputs are disabled");
                    Thread.Sleep(span);
                }
                finally
                {
                    NativeMethods.BlockInput(false);
                    Console.WriteLine("inputs are enabled");
                }
            }
        }

        public static AutomationElement calculatorAutomationElement;
        public static AutomationElementCollection elements;
        public static Dictionary<String, String> answers;
        public static String startFileAddress = @"C:\ProgramData\CentERdata\SHARE_CASE_CTRL_W8_1\blaise\start.bat";

       [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);



        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String scenariosDirectory = @"scenarios to test";
            DirectoryInfo d = new DirectoryInfo(scenariosDirectory);
            
            String newTestCase = "";
            String testCaseAddress = "";

            System.IO.StreamWriter outputFile = new System.IO.StreamWriter(@"log.txt");

            FileInfo[] files = d.GetFiles("*.txt");


            for (int i=0; i<files.Length; i++)
            {
                Console.WriteLine("-------------------------NEW ATTEMPT");
                outputFile.WriteLine("");

                FileInfo file = files[i];
                testCaseAddress = scenariosDirectory + "\\" + file.Name;
                readAnswers(testCaseAddress);

                //read new test case
                try {
                    newTestCase = answers["test case"];
                    Console.WriteLine("-------------------------test case #" + newTestCase);
                    outputFile.WriteLine("test case #" + newTestCase);
                }
                catch {
                    Console.WriteLine("-------------------------Please, add the test case number to the file \"" + file.Name + "\"");
                    outputFile.WriteLine("Please, add the test case number to the file \"" + file.Name + "\"");
                    continue;
                }


                //update test case
                try {
                    updateCurrentTestCase(newTestCase);
                }
                catch {
                    Console.WriteLine("-------------------------Couldn't update the test case number");
                    outputFile.WriteLine("Couldn't update the test case number");
                    continue;
                    
                }
             
                //open the test case
                try
                {
                    runFile(@"C:\ProgramData\CentERdata\SHARE_CASE_CTRL_W8_1\blaise\start.bat", @"C:\ProgramData\CentERdata\SHARE_CASE_CTRL_W8_1\blaise");
                    activateProcess("Dep");
                    Thread.Sleep(15000);
                    makeWindowAutomationElement(titleOfCAPIwindow());
                    //makeWindowAutomationElement("SHARE w8 - 50+ in Europe - Version 8.1.12 - NewTssfedst4-1");
                   
                }

                catch
                {
                    Console.WriteLine("-------------------------Couldn't open the test case " + newTestCase);
                    outputFile.WriteLine("Couldn't open the test case " + newTestCase);
                    closeCurrentWindow();
                }
        
                //answer questions
                String currentQuestion = "";
          
                try
                {
               
                    while (true)
                    {
                        currentQuestion = findQuestionText().ToLower();
                        Console.WriteLine("This is the question: "+currentQuestion);
                      
                        if (currentQuestion == "")
                            break;
                        String currentAnswer = answers[currentQuestion];
                        Console.WriteLine("The Answer is: "+ currentAnswer);

                        if (currentAnswer != "next")
                            typeAnswer(currentAnswer);

                        if (currentAnswer == "skip"){
                            Thread.Sleep(61000);
                        }

                        //select_control("Next");
                        System.Windows.Forms.SendKeys.SendWait("{RIGHT}");
                        Thread.Sleep(500);
                    }

                    Console.WriteLine("-------------------------Test case successfully done");
                    outputFile.WriteLine("Test case successfully done");
                    File.Delete(testCaseAddress);
                    executePython();
                  
                }
                catch {
                    Console.WriteLine("-------------------------CAPI stoped at the question: " + currentQuestion);
                    outputFile.WriteLine("CAPI stoped at the question: " + currentQuestion);
                }


                try
                {
                    closeCurrentWindow();
                    
                }
                catch { }


            }

            outputFile.Flush();
            outputFile.Close();
            Application.Exit();
            restartProgram();

        }


        public static void readAnswers(String fileName)
        {
            answers = new Dictionary<String, String>();


            try
            {
                var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        try
                        {
                            string[] tokens = line.Split('\t');
                            String question = tokens[0].ToLower();
                            String answer = tokens[1];
                            
                            answers.Add(question, answer);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("couldn't open file with scenario");
            }
        }

        public static String getCurrentTestCase() {
            string text = File.ReadAllText(startFileAddress);
            int indexOfCaseNumber = text.IndexOf("TTtest");
            return text.Substring(indexOfCaseNumber + 6, 3);
        }

        public static void updateCurrentTestCase(String newTestCase)
        {
            Console.WriteLine("check 1.1");
            string text = File.ReadAllText(startFileAddress);
            Console.WriteLine("check 1.2");
            text = text.Replace("TTtest" + getCurrentTestCase(), "TTtest" + newTestCase);
            Console.WriteLine("check 1.3");
            File.WriteAllText(startFileAddress, text);
            Console.WriteLine("check 1.4");
        }
        
        public static void runFile(String fileName, String directory) {
            Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.WorkingDirectory = directory;
            proc.Start();
        }


      

        private void executePython()
        {
            System.Diagnostics.Process.Start(@"R:\Development\wave8\2_Testing\Innovation\KSIM\KSIM_on_B5_(ver. 7.0)\KSIM_on_B5(ver. 1.0)\bin\Debug\batchTesting\demo_batch.exe");
        }



        private void restartProgram()
        {
            // Get file path of current process 
            var filePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //var filePath = Application.ExecutablePath;  // for WinForms

            // Start program
            Process.Start(filePath);

            // For Windows Forms app
            Application.Exit();

            // For all Windows application but typically for Console app.
            Environment.Exit(0);
        }


        public static void activateProcess(String processName) {
                        
            int time_over = 0;
            while (true)
            {
                time_over++;
                //Console.WriteLine(time_over);

                if (time_over > 60000)
                {
                    MessageBox.Show("Couldn't run the app, please try again.");
                    return;
                }

                else
                {
                    var prc = Process.GetProcessesByName(processName);
                    if (prc.Length > 0)
                    {
                        SetForegroundWindow(prc[0].MainWindowHandle);
                        //Console.WriteLine("Set as a foreground!");
                        break;
                    }
                }
            }
        }

        public static void makeWindowAutomationElement(String name)
        {
            calculatorAutomationElement = AutomationElement.RootElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, name));

            if (calculatorAutomationElement != null)
            {
                //MessageBox.Show(calculatorAutomationElement.ToString());
            }
            else
                MessageBox.Show("can't find an app");
        }


        

    public static void closeCurrentWindow() {
            elements = calculatorAutomationElement.FindAll(TreeScope.Descendants, Condition.TrueCondition);
            foreach (AutomationElement ae in elements)
            {
                try
                {
                    if (GetText(ae).Equals("Close"))
                    {
                        GetInvokePattern(ae).Invoke();
                        break;
                    }
                }
                catch { }
            }
        }


        public static String currentQuestionFull()
        {
            String title = findQuestionText();
            int size = title.Length;
            int lastDotPosition = -1;

            for (int i = size - 1; i >= 0; i--)
                if (title[i] == '.')
                {
                    lastDotPosition = i;
                    break;
                }

            return title.Substring(lastDotPosition + 1).ToLower();
        }

        public static String findQuestionText() {

            elements = calculatorAutomationElement.FindAll(TreeScope.Descendants, Condition.TrueCondition);

            foreach (AutomationElement ae in elements)
            {
                
                try
                {
                    if (GetText(ae).ToLower().StartsWith("sec") || GetText(ae).ToLower().StartsWith("test") || GetText(ae).ToLower().StartsWith("intro"))
                    {
                        return GetText(ae);
                    } 
                    
                }
                catch {
                }
            }
            Console.WriteLine("coudn't find a question control");
            return "";
        }

        public static string GetText(AutomationElement element)
        {
            object patternObj;
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out patternObj))
            {
                var valuePattern = (ValuePattern)patternObj;
                return valuePattern.Current.Value;
            }
            else if (element.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
            {
                var textPattern = (TextPattern)patternObj;
                return textPattern.DocumentRange.GetText(-1).TrimEnd('\r'); // often there is an extra '\r' hanging off the end.
            }
            else
            {
                return element.Current.Name;
            }
        }


        public static void select_control(String name_needed)
        {
            elements = calculatorAutomationElement.FindAll(TreeScope.Descendants, Condition.TrueCondition);
            foreach (AutomationElement ae in elements)
            {
                try
                {
                    String name = ae.GetCurrentPropertyValue(AutomationElement.NameProperty) as string;

                    if (name.StartsWith(name_needed))// || GetText(ae).StartsWith(name_needed))
                    {
                        //Console.WriteLine("next found!");
                        ae.SetFocus();
                        //Thread.Sleep(1000);
                        System.Windows.Forms.SendKeys.SendWait(" ");
                        //Console.WriteLine("control is setted");

                        break;
                    }
                    else { }

                }
                catch
                {
                    MessageBox.Show("can't check the element");
                    return;
                }

            }

        }

        public static void typeAnswer(String answer) {
            //Answer required
            elements = calculatorAutomationElement.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.HasKeyboardFocusProperty, true));

            object valuePattern = null;
            foreach (AutomationElement ae in elements)
            {
                //Console.WriteLine("control here!");  
                //ae.SetFocus();              
                try
                {
                    if (ae.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
                    {
                        ((ValuePattern)valuePattern).SetValue(answer);
                        //Console.WriteLine("value is set");
                       
                    }  
                }
                catch
                {
                    MessageBox.Show("Error occured");
                    SendKeys.Send("{ESC}");
                    //Console.WriteLine("couldn't set a value");
                    return;
                }
            }
        }

        public static InvokePattern GetInvokePattern(AutomationElement element)
        {
            //https://www.codeproject.com/Articles/141842/Automate-your-UI-using-Microsoft-Automation-Framew
            return element.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
        }
       
        /*public static String currentQuestionFull()
        {
            String title = GetActiveWindowTitle();
            int size = title.Length;
            int lastDotPosition = 0;

            for (int i = size - 1; i >= 0; i--)
                if (title[i] == '.')
                {
                    lastDotPosition = i;
                    break;
                }

            return title.Substring(lastDotPosition + 1);
        }*

        private static string GetActiveWindowTitle()
        {
            //https://stackoverflow.com/questions/115868/how-do-i-get-the-title-of-the-current-active-window-using-c
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }
        */

        public void showAllRunning() {
                    var allProcceses = Process.GetProcesses();

                    foreach (Process p in allProcceses)
                        MessageBox.Show(p.ToString() + "///" + p.ProcessName + "///" + p.MainWindowTitle);
                }

        public string titleOfCAPIwindow()
        {
            var allProcceses = Process.GetProcesses();

            foreach (Process p in allProcceses)
                if (p.MainWindowTitle.StartsWith("SHARE"))
                    return p.MainWindowTitle;
                    
                return null;
               
        }


        public static List<Control> FindTheControls(List<Control> foundSofar, Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                foundSofar.Add(c);

                if (c.Controls.Count > 0)
                    {
                        return FindTheControls(foundSofar, c);
                    }
            }

            return foundSofar;
        }

        public void print_all_controls()
                {
                    elements = calculatorAutomationElement.FindAll(TreeScope.Descendants, Condition.TrueCondition);
                    foreach (AutomationElement ae in elements)
                    {
                        try
                        {
                            String name = ae.GetCurrentPropertyValue(AutomationElement.ClassNameProperty) as string;
                            Console.WriteLine(GetText(ae) + "/" + name);

                        }
                        catch
                        {
                            MessageBox.Show("can't check the element");
                            return;
                        }

                    }

                }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

   

    
}
