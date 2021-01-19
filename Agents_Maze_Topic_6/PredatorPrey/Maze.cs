using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AgentsMaze
{
    public enum CellState { Floor, Agent, Wall, Exit };

    public enum Direction { Up, Down, Left, Right };

    public class Maze : ActressMas.TurnBasedEnvironment
    {
        private Cell[][] _map;
        private int _currentId;
        private List<string> _activeAgents;
        private List<Direction> directions = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToList();

        MainForm mainForm;

        private void FormThread()
        {
            mainForm.ShowDialog();
        }

        public Maze(int numberOfTurns = 0, int delayAfterTurn = 0, bool randomOrder = true)
            : base(numberOfTurns, delayAfterTurn, randomOrder)
        {
        }

        public void InitMazeMap()
        {
            _map = new Cell[Utils.GridSize][];
            for (int i = 0; i < Utils.GridSize; i++)
            {
                _map[i] = new Cell[Utils.GridSize];
                for (int j = 0; j < Utils.GridSize; j++)
                    _map[i][j] = new Cell(j, i);

            }
            _currentId = 0;

            mainForm = new MainForm(_map);
            Thread t = new Thread(new ThreadStart(FormThread));
            t.Start();
        }

        public void UpdateUI(int turn)
        {
            mainForm.Refresh();
        }

        //construim labirintul
        public void AddWallsAndExitToMaze()
        {
            String input = File.ReadAllText("maze25x25.txt");

            int i = 0, j = 0;

            foreach (var row in input.Split('\n'))
            {
                j = 0;
                foreach (var col in row.Trim().Split(' '))
                {
                    if (int.Parse(col.Trim()) == 0)
                        _map[j][i].State = CellState.Floor;
                    if (int.Parse(col.Trim()) == 1)
                        _map[j][i].State = CellState.Wall;
                    if (int.Parse(col.Trim()) == 2)
                        _map[j][i].State = CellState.Exit;
                    j++;
                }
                i++;
            }
        }

        //adaugam agentii in labirint
        public void AddAgentToMaze(Agent a)
        {
            //lista cu toate celulele libere (de tip Floor)
            List<Cell> floorCells = _map.SelectMany(row => row.Where(item => item.State != CellState.Wall || item.State != CellState.Exit || item.State != CellState.Agent)).ToList();
            //plasam agentul random in una din celulele libere
            Cell cell = floorCells[Utils.Rand.Next(floorCells.Count() - 1)];

            if (a.GetType().Name == "Agent")
                cell.State = CellState.Agent;

            a.Cell = cell;
            cell.AgentInCell = a;
        }

        public string CreateName(Agent a)
        {
            if (a.GetType().Name == "Agent")
                return string.Format("A{0}", _currentId++);

            throw new Exception("Unknown agent type: " + a.GetType().ToString());
        }

        public void StartNewTurn()
        {
            _activeAgents = new List<string>();

            for (int i = 0; i < Utils.GridSize; i++)
                for (int j = 0; j < Utils.GridSize; j++)
                {
                    if (_map[i][j].State == CellState.Agent)
                        _activeAgents.Add(_map[i][j].AgentInCell.Name);
                }
        }

        public string GetNextAgent()
        {
            if (_activeAgents.Count != 0)
            {
                int r = Utils.Rand.Next(_activeAgents.Count);
                string name = _activeAgents[r];
                _activeAgents.Remove(name);
                return name;
            }
            else
                return "";
        }

        public void Move(Agent a, Cell cel)
        {
            // moving the agent

            Cell cell = GetCell(cel.Column, cel.Line);
            Cell agentCell = GetCell(a.Cell.Column, a.Cell.Line);


            cell.State = agentCell.State;
            cell.AgentInCell = agentCell.AgentInCell;

            agentCell.State = CellState.Floor;
            agentCell.AgentInCell = null;

            // updating agent position

            Cell newCell = new Cell(cell.Line, cell.Column);
            newCell.Weight += 1;
            a.Cell = newCell;
            a.Path.Add(newCell);
        }

        public void WriteToUI(string text)
        {
            mainForm.AddText(text + "\r\n");
        }

        public Cell GetDirection(Agent a)
        {
            // test each direction to find the best score

            List<Cell> viableCells = directions.Select(direction =>
            {
                int row = 0;
                int col = 0;

                switch (direction)
                {
                    case Direction.Up:
                        row = a.Cell.Line - 1;
                        col = a.Cell.Column;
                        break;
                    case Direction.Down:
                        row = a.Cell.Line + 1;
                        col = a.Cell.Column;
                        break;
                    case Direction.Left:
                        row = a.Cell.Line;
                        col = a.Cell.Column - 1;
                        break;
                    case Direction.Right:
                        row = a.Cell.Line;
                        col = a.Cell.Column + 1;
                        break;
                }

                if (!IsValidCell(col, row))
                {
                    return null;
                }

                Cell cell = GetCell(col, row);
                
                if (IsWall(cell))
                {
                    return null;
                }
                else if (IsExplored(a, cell) && IsNotDeadEnd(a, cell))
                {
                    a.Path.Remove(cell);
                    cell.Weight -= 1;
                    return cell;
                }
                else
                {
                    return cell;
                }
            }).ToList();

            //alege directia cu cel mai mare weight neocupata de alt agent
            Cell bestChoice = viableCells.Max();

            return bestChoice;
        }

        public bool IsExit(Cell cell)
        {
            return cell.State == CellState.Exit;
        }

        private bool IsNotDeadEnd(Agent a, Cell cell)
        {
            if (IsWall(cell))
            {
                return false;
            }
            return true;
        }

        private bool IsExplored(Agent a, Cell cell)
        {
            return a.Path.Contains(cell);
        }

        private Cell GetCell(int col, int row)
        {
            return _map[col][row];
        }

        private bool IsWall(Cell cell)
        {
            if (cell == null) return false;
            return cell.State == CellState.Wall || cell.State == CellState.Agent;
        }

        private int GetHeight()
        {
            return Utils.GridSize;
        }

        private int GetWidth()
        {
            return Utils.GridSize;
        }

        private bool IsValidCell(int col, int line)
        {
            if (line <= 0 || line >= GetHeight()-1 || col <= 0 || col >= GetWidth()-1)
            {
                return false;
            }
            return true;
        }
    }

    public class Cell : IEquatable<Cell>, IComparable<Cell>
    {
        public CellState State;
        public Agent AgentInCell;
        public int Line, Column;
        public int Weight;

        public Cell(int line, int column)
        {
            State = CellState.Floor;
            AgentInCell = null;
            Line = line;
            Column = column;
        }

        public int CompareTo(Cell other)
        {
            if (other == null) return -1;
            return Weight.CompareTo(other.Weight);
        }

        public bool Equals(Cell other)
        {
            if (other == null) return false;
            return Column == other.Column && Line == other.Line && State == other.State;
        }
		
		public int GetHashCode(Cell cell)
        {
            return cell.Line.GetHashCode() ^ cell.Column.GetHashCode();
        }
    }
}