using ActressMas;
using System;
using System.Collections.Generic;

namespace AgentsMaze
{
    public class Agent : TurnBasedAgent
    {
        public Cell Cell;
        protected Maze _mazeEnv;
        public List<Cell> Path = new List<Cell>();

        public override void Setup()
        {
            _mazeEnv = (Maze)this.Environment;

            if (Utils.Verbose)
                Console.WriteLine("Agent {0} started in ({1},{2})", this.Name, Cell.Column, Cell.Line);
        }

        public override void Act(Queue<Message> messages)
        {
            if (messages.Count > 0)
            {
                Message message = messages.Dequeue();
                if (Utils.Verbose)
                    Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                if (TryToMove())
                {
                    Send("scheduler", "done");
                }

            }
        }

        protected bool TryToMove()
        {
            Cell cell = _mazeEnv.GetDirection(this);

            if (cell != null)
            {
                if (_mazeEnv.IsExit(cell))
                {
                    Broadcast("I found the exit...", true);
                    _mazeEnv.Remove(this);
                    return false;
                }
                else
                {
                    if (Utils.Verbose)
                        Console.WriteLine("Moving " + this.Name);

                    _mazeEnv.Move(this, cell);
                    return true;
                }
            }
            else
                return false;
        }

    }
}