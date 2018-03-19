using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRecordingApp.TestLogging
{
    public class LogRecord
    {
        string _participantID;
        double _startInMillisecond;
        double _stopVideoInMillisecond;
        double _answerInMillisecond;
        double _confidenceInMillisecond;
        string _seatPos;
        string _layout;
        string _interface;
        string _stimulus;
        string _answer;
        int _confidence;

        public string ParticipantID
        {
            get
            {
                return _participantID;
            }

            set
            {
                _participantID = value;
            }
        }

        public double StartInMillisecond
        {
            get
            {
                return _startInMillisecond;
            }

            set
            {
                _startInMillisecond = value;
            }
        }

        public double StopVideoInMillisecond
        {
            get
            {
                return _stopVideoInMillisecond;
            }

            set
            {
                _stopVideoInMillisecond = value;
            }
        }

        public double AnswerInMillisecond
        {
            get
            {
                return _answerInMillisecond;
            }

            set
            {
                _answerInMillisecond = value;
            }
        }

        public string SeatPos
        {
            get
            {
                return _seatPos;
            }

            set
            {
                _seatPos = value;
            }
        }

        public string Layout
        {
            get
            {
                return _layout;
            }

            set
            {
                _layout = value;
            }
        }

        public string Interface
        {
            get
            {
                return _interface;
            }

            set
            {
                _interface = value;
            }
        }

        public string Stimulus
        {
            get
            {
                return _stimulus;
            }

            set
            {
                _stimulus = value;
            }
        }

        public string Answer
        {
            get
            {
                return _answer;
            }

            set
            {
                _answer = value;
            }
        }

        public int Confidence
        {
            get
            {
                return _confidence;
            }

            set
            {
                _confidence = value;
            }
        }

        public double ConfidenceInMillisecond
        {
            get
            {
                return _confidenceInMillisecond;
            }

            set
            {
                _confidenceInMillisecond = value;
            }
        }

        public override string ToString()
        {
            return String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", _participantID, _startInMillisecond, _stopVideoInMillisecond, _answerInMillisecond,_confidenceInMillisecond,
                                     _seatPos,_interface, _stimulus, _answer, _confidence);
        }
        public static string getPropertiesNames()
        {
            return "ParticipantID,Start,StopVideo,AnswerTime,ConfidenceTime,SeatPos,Interface,Stimulus,Answer,Confidence";
        }
    }
}
