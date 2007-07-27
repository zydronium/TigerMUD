using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    public class Question
    {
        protected PlayerCharacter pc;
        protected string questionprompt = string.Empty;
        protected string response = string.Empty;
        protected List<Question> childquestion = new List<Question>();

        public Question()
        {

        }

        public Question(PlayerCharacter pcin, GameContext gamecontext, string questionpromptin)
        {
            pc = pcin;
            pc.Question = this;
            pc.ConnectionState = GameLibrary.ConnectionState.QuestionPrompt;
            questionprompt = questionpromptin;


        }

        public virtual bool Show()
        {
            try
            {
                pc.Send(questionprompt);
                pc.Question = this;
                pc.ConnectionState = GameLibrary.ConnectionState.QuestionResponse;
            }
            catch
            {
                return true;
            }
            return false;
        }

        public virtual void HandleResponse(string responsein)
        {
            if (String.IsNullOrEmpty(responsein)) return;
            if (responsein == this.ExitResponse) return;
            if (responsein == this.BackResponse)
            {
                if (Parent != null)
                {
                    pc.Question = Parent;
                    Parent.Show();
                }
                return;
            }

            // Find the child question associated with this response if applicable
            ProcessChoices(responsein);
            return;
        }


        public virtual void ProcessChoices(string responsein)
        {
            CheckForChildQuestion();
            return;
        }

        public virtual void CheckForChildQuestion()
        {
            // Show child question if it exists
            if (childquestion.Count > 0) childquestion[0].Show();
            
            return;
        }

        public virtual bool AddChildQuestion(Question newquestion)
        {
            childquestion.Add(newquestion);
            return false;
        }

        public virtual bool RemoveChildQuestion(Question newquestion)
        {
            childquestion.Remove(newquestion);
            return false;
        }

        private string exitresponse;

        public string ExitResponse
        {
            get { return exitresponse; }
            set { exitresponse = value; }
        }

        private string backresponse;

        public string BackResponse
        {
            get { return backresponse; }
            set { backresponse = value; }
        }

        private Question parent;

        public Question Parent
        {
            get { return parent; }
            set { parent = value; }
        }


    }
}
