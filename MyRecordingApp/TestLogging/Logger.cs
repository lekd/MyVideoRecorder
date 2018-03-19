using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRecordingApp.TestLogging
{
    public class Logger
    {
        public const string LOG_STOP = "stop";
        public const string LOG_TARGET_CHOOSE = "target-click";
        public const string LOG_CONFIDENCE_CHOOSE = "confidence-click";
        public delegate void CurrentRecordSavedEventHandler();
        string _curParticipantID;
        string _curLayout;
        string _curSeatPos;
        string _curInterface;
        LogRecord curRecord;
        StreamWriter fileWriter;
        string logFilePath = string.Empty;
        public event CurrentRecordSavedEventHandler recordSavedEventHandler = null;
        public Logger()
        {
            //initialize a file name
            //first create the log-file folder
            string logFileFolder = Environment.CurrentDirectory + "/Log";
            if(!Directory.Exists(logFileFolder))
            {
                Directory.CreateDirectory(logFileFolder);
            }
            logFilePath = logFileFolder + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";
            fileWriter = new StreamWriter(logFilePath);
            fileWriter.WriteLine(LogRecord.getPropertiesNames());
        }
        public void setGlobalLogParams(string[] globalParams)
        {
            _curParticipantID = globalParams[0];
            _curSeatPos = globalParams[1];
            _curLayout = globalParams[2];
            _curInterface = globalParams[3];
        }
        public void startNewRecord(string stimulus)
        {
            curRecord = new LogRecord();
            curRecord.ParticipantID = _curParticipantID;
            curRecord.SeatPos = _curSeatPos;
            curRecord.Layout = _curLayout;
            curRecord.Interface = _curInterface;
            curRecord.Stimulus = stimulus;
        }
        public void setRecordStartTime(double startTime)
        {
            curRecord.StartInMillisecond = startTime;
        }
        public void setRecordStopTime(double stopTime)
        {
            curRecord.StopVideoInMillisecond = stopTime;
        }
        public void setRecordAnswerTime(double answerTime)
        {
            curRecord.AnswerInMillisecond = answerTime;
        }
        public void setRecordAnswer(string answer)
        {
            curRecord.Answer = answer;
        }
        public void setRecordConfidence(int confidence)
        {
            curRecord.Confidence = confidence;
        }
        public void setConfidenceAnswerTime(double confidenceAnsTime)
        {
            curRecord.ConfidenceInMillisecond = confidenceAnsTime;
        }
        public void saveCurrentRecord()
        {
            fileWriter.WriteLine(curRecord.ToString());
            if(recordSavedEventHandler != null)
            {
                recordSavedEventHandler();
            }
        }
        public void Close()
        {
            try { 
                fileWriter.Close();
                string newFilePath = string.Format("{0}/{1}/{2}_{3}_{4}_{5}.csv", Environment.CurrentDirectory, "Log", _curParticipantID, _curSeatPos, _curLayout, _curInterface);
                File.Move(logFilePath, newFilePath);
                }
            catch(Exception ex)
            {

            }
        }
        public static DateTime BeginningOfToday()
        {
            return DateTime.Parse(String.Format("{0}-{1}-{2} 0:00:00", DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day));
        }
    }
}
