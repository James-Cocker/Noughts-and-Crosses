using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;



namespace NoughtAndCross
{
    public partial class Form1 : Form
    {

        // Hello! All the pixel art in this program is my own. I would consider the special 
        // features of my game to be the graphics and putting the different bots against themselves 
        // as its fun to watch, hope you enjoy it!


        // Create a media player called 'player'
        WindowsMediaPlayer player = new WindowsMediaPlayer();
        
        // Defining basic variabled needed as globals
        // Noughts will always go first as player one, therefore if P1sGo = true
        bool P1sGo = true;
        Random Rand = new Random();
        string Player1, Player2, Winner;
        int[,] Grid = new int[,] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        int[,] WeightedGrid = new int[,] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        bool ComputerGame = false, ComputerGameWon = false, NormalGameWon = false, NormalGame = false;              // Have a go at putting the computer against itself
        int Volume, RowTotal, ColumnTotal, TotalFilled, ImageP1, ImageP2, HighestX, HighestY, HighestWeight;

        // Animated character image set
        Image[] ImagesP2 = new Image[] { Properties.Resources.P2_12, Properties.Resources.P2_22, Properties.Resources.P2_32, Properties.Resources.P2_42 };
        Image[] ImagesP1 = new Image[] { Properties.Resources.P1_12, Properties.Resources.P1_22, Properties.Resources.P1_32, Properties.Resources.P1_42, Properties.Resources.P1_52, Properties.Resources.P1_62 };



        // --- Subroutines ---
 

        // Creating a 'weighted grid', a 3x3 2d array used by the computer to take a move depending on how good that move is (where the higher the number the better).
        // The values used on this are based and tweated from previous games played and should enable the computer to make the best possible move possible.
        // Here is is taking in the starting x and y values (as the same subroutine is used for both columns, rows and diagonals), then the offsets (how much the x and y values need to be incremented)
        // each time to get to the next value we want (bare in mind 0 is nothing, 1 is an X, and 10 is a O). It then will also take in the weighted grid and ammend it each time the subroutine call itself.
        void Weighting(int X, int Y, int OffsetX, int OffsetY) // - make sure to set line to 1 before function is initially called
        {
            // Reset the number of Xs and Os on each line to be 0
            int Xs = 0;
            int Os = 0;

            // As the value of x and y are changes (like in the for loop below they are incremented) keep X and Y as constants
            int x = X;
            int y = Y;

            // Work out the number of Xs and Os in the line
            for (int Loop = 0; Loop <= 2; Loop++)
            {
                if (Grid[x, y] == 1)
                {
                    Xs += 1;
                }
                else if (Grid[x, y] == 10)
                {
                    Os += 1;
                }

                // Add the offset onto the next iteration
                x += OffsetX;
                y += OffsetY;
            }

            // As the value of x and y are changes reset back to original X and Y
            x = X;
            y = Y;

            // Assign values to weighted grid
            for (int Loop = 0; Loop <= 2; Loop++)
            {
                // If there is nothing on that row give it a weighting of 0
                if (Xs == 0 && Os == 0)
                {
                    WeightedGrid[x, y] += 1;
                }
                // If it is P1s go they must be x, assign the slots for line '[their piece] - nothing - nothing'
                else if ((Xs == 1 && Os == 0 && !P1sGo) || (Xs == 0 && Os == 1 && P1sGo))
                {
                    WeightedGrid[x, y] += 2;
                }
                // Assign the slots for line '[their piece] - [their piece] - nothing' - they will win if they make the move on this space
                else if ((Xs == 2 && Os == 0 && !P1sGo) || (Xs == 0 && Os == 2 && P1sGo))
                {
                    WeightedGrid[x, y] += 100;
                }
                // Assign the slots for line '[their piece] - [opponants piece] - nothing'
                else if (Xs == 1 && Os == 1)
                {
                    WeightedGrid[x, y] += 0;
                }
                // Assign the slots for line '[opponants piece] - nothing - nothing'
                else if ((Xs == 0 && Os == 1 && !P1sGo) || (Xs == 1 && Os == 0 && P1sGo))
                {
                    WeightedGrid[x, y] += 1;
                }
                // Assign the slots for line '[opponants piece] - [opponants piece] - nothing' - they must defend this move (unless they can win)
                else if ((Xs == 0 && Os == 2 && !P1sGo) || (Xs == 2 && Os == 0 && P1sGo))
                {
                    WeightedGrid[x, y] += 50;
                }
                // There is no need to analyse lines such as 'x - o - x' (no spare spaces available to play) as they will get overwritten

                // Add the offset onto the next iteration
                x += OffsetX;
                y += OffsetY;
            }
            // We want to iterate through the above code 8 times (3 columns, 3 rows, 2 diagonals), so we shall call the subroutine inside itself with different starting positions and offsets).
            // The code starts on line 2 as it already has gone though it once --- line 1 is (0, 0, 1, 0) as it starts at [0,0] and works its way down the y value once each iteration (starts on columns)

            // Just to check the program if you are curious to do - put back in

            //PrintGrid("Grid", Grid);                  // 1. (uncomment these two lines)
            //PrintGrid("Weighted grid", WeightedGrid); // 2. 

            // Overwite all taken squares to be '-1' as they cannot be played
            for (x = 0; x <= 2; x++)
            {
                for (y = 0; y <= 2; y++)
                {
                    if (Grid[x, y] != 0)
                    {
                        WeightedGrid[x, y] = -1;
                    }
                }
            }
        }

        // Print grid given
        void PrintGrid(string GridName, int[,] Grid)
        {
            Console.WriteLine("");
            Console.WriteLine(GridName);
            Console.WriteLine(Grid[0, 0].ToString() + " | " + Grid[0, 1].ToString() + " | " + Grid[0, 2].ToString());
            Console.WriteLine(Grid[1, 0].ToString() + " | " + Grid[1, 1].ToString() + " | " + Grid[1, 2].ToString());
            Console.WriteLine(Grid[2, 0].ToString() + " | " + Grid[2, 1].ToString() + " | " + Grid[2, 2].ToString());
            Console.WriteLine("");
        }

        // A small array to take in the grid value and give a text output to the console (used to help show grid better)
        string DisplayGrid(int n)
        {
            if (n == 0)
            {
                return "-";
            }
            else if (n == 1)
            {
                return "x";
            }
            else
            {
                return "o";
            }
        }

        void CheckWin()
        {
            // Check accross for a win
            for (int Row = 0; Row <= 2; Row++)
            {
                // Reset total to 0 for each row
                RowTotal = 0;

                for (int Column = 0; Column <= 2; Column++)
                {
                    RowTotal += Grid[Row, Column];
                }

                if (RowTotal == 30)
                {
                    // noughts win
                    lblP1Score.Text = (Int32.Parse(lblP1Score.Text) + 1).ToString();
                    GameWon();
                    Winner = "Noughts";
                }
                else if (RowTotal == 3)
                {
                    // crosses win
                    lblP2Score.Text = (Int32.Parse(lblP2Score.Text) + 1).ToString();
                    GameWon();
                    Winner = "Crosses";
                }
            }

            // Check columns for a win
            for (int Column = 0; Column <= 2; Column++)
            {
                // Reset total to 0 for each column
                ColumnTotal = 0;

                for (int Row = 0; Row <= 2; Row++)
                {
                    ColumnTotal += Grid[Row, Column];
                }

                if (ColumnTotal == 30)
                {
                    // noughts win
                    lblP1Score.Text = (Int32.Parse(lblP1Score.Text) + 1).ToString();
                    GameWon();
                    Winner = "Noughts";
                }
                else if (ColumnTotal == 3)
                {
                    // crosses win
                    lblP2Score.Text = (Int32.Parse(lblP2Score.Text) + 1).ToString();
                    GameWon();
                    Winner = "Crosses";
                }
            }

            // Check diagonals for win
            int Diagonal1Total = Grid[0, 0] + Grid[1, 1] + Grid[2, 2];
            int Diagonal2Total = Grid[2, 0] + Grid[1, 1] + Grid[0, 2];

            if ((Diagonal1Total == 30) || (Diagonal2Total == 30))
            {
                // noughts win
                lblP1Score.Text = (Int32.Parse(lblP1Score.Text) + 1).ToString();
                GameWon();
                Winner = "Noughts";
            }
            else if ((Diagonal1Total == 3) || (Diagonal2Total == 3))
            {
                // crosses win
                lblP2Score.Text = (Int32.Parse(lblP2Score.Text) + 1).ToString();
                GameWon();
                Winner = "Crosses";
            }


            // Check for a tie
            TotalFilled = 0;
            for (int Row = 0; Row <= 2; Row++)
            {
                for (int Column = 0; Column <= 2; Column++)
                {
                    if(Grid[Row, Column] != 0)
                    {
                        TotalFilled += 1;
                    }
                }
            }

            if (TotalFilled == 9)
            {
                // draw
                GameWon();
                Winner = "Draw";
            }

            // As check win is called with both bots and players, switch over their bold titling and update scoring here
            CorrectLabelColours();
        }

        // Placing the correstponding X or O depending on whether it is P1sGo
        void PlaceMove(int x, int y, bool IsNoughts)
        {
            if (!P1sGo)
            {
                if (x == 0 && y == 0)
                {
                    lblPos1.Image = Properties.Resources.X__2;
                    btnPos1.Visible = false;
                }
                else if (x == 0 && y == 1)
                {
                    lblPos2.Image = Properties.Resources.X__2;
                    btnPos2.Visible = false;
                }
                else if (x == 0 && y == 2)
                {
                    lblPos3.Image = Properties.Resources.X__2;
                    btnPos3.Visible = false;
                }
                else if (x == 1 && y == 0)
                {
                    lblPos4.Image = Properties.Resources.X__2;
                    btnPos4.Visible = false;
                }
                else if (x == 1 && y == 1)
                {
                    lblPos5.Image = Properties.Resources.X__2;
                    btnPos5.Visible = false;
                }
                else if (x == 1 && y == 2)
                {
                    lblPos6.Image = Properties.Resources.X__2;
                    btnPos6.Visible = false;
                }
                else if (x == 2 && y == 0)
                {
                    lblPos7.Image = Properties.Resources.X__2;
                    btnPos7.Visible = false;
                }
                else if (x == 2 && y == 1)
                {
                    lblPos8.Image = Properties.Resources.X__2;
                    btnPos8.Visible = false;
                }
                else if (x == 2 && y == 2)
                {
                    lblPos9.Image = Properties.Resources.X__2;
                    btnPos9.Visible = false;
                }

                Grid[x, y] = 1;
            }

            else
            {
                if (x == 0 && y == 0)
                {
                    lblPos1.Image = Properties.Resources.O___2;
                    btnPos1.Visible = false;
                }
                else if (x == 0 && y == 1)
                {
                    lblPos2.Image = Properties.Resources.O___2;
                    btnPos2.Visible = false;
                }
                else if (x == 0 && y == 2)
                {
                    lblPos3.Image = Properties.Resources.O___2;
                    btnPos3.Visible = false;
                }
                else if (x == 1 && y == 0)
                {
                    lblPos4.Image = Properties.Resources.O___2;
                    btnPos4.Visible = false;
                }
                else if (x == 1 && y == 1)
                {
                    lblPos5.Image = Properties.Resources.O___2;
                    btnPos5.Visible = false;
                }
                else if (x == 1 && y == 2)
                {
                    lblPos6.Image = Properties.Resources.O___2;
                    btnPos6.Visible = false;
                }
                else if (x == 2 && y == 0)
                {
                    lblPos7.Image = Properties.Resources.O___2;
                    btnPos7.Visible = false;
                }
                else if (x == 2 && y == 1)
                {
                    lblPos8.Image = Properties.Resources.O___2;
                    btnPos8.Visible = false;
                }
                else if (x == 2 && y == 2)
                {
                    lblPos9.Image = Properties.Resources.O___2;
                    btnPos9.Visible = false;
                }

                Grid[x, y] = 10;
            }

            // Check for win then invert the player's turn
            CheckWin();
            P1sGo = !P1sGo;
        }

        void EasyBotMove()
        {
            int Counter = 0;
            int Rand1, Rand2;

            do
            {
                Rand1 = Rand.Next(3);
                Rand2 = Rand.Next(3);

                Counter++;

            } while ((Grid[Rand1, Rand2] != 0) && (Counter <= 100));

            PlaceMove(Rand1, Rand2, true);

            // If its computer vs computer, chuck it back into checking the computer move
            if (ComputerGame)
            {
                CheckComputerMove();
            }
        }

        void MediumBotMove(bool Crosses)
        {
            // Get a random number between 0 and 1 to decide whether to make a good, or random move
            int RandNum = Rand.Next(2);
            
            if (RandNum == 0)
            {
                EasyBotMove();
            }
            else
            {
                HardBotMove(Crosses);
            }

            // If its computer vs computer, chuck it back into checking the computer move
            if (ComputerGame)
            {
                CheckComputerMove();
            }
        }

        void HardBotMove(bool Crosses)
        {
            // Clear weighted grid and highest x and y values
            HighestX = 0;
            HighestY = 0;
            HighestWeight = 0;
            ClearGrid(WeightedGrid);

            // Columns
            Weighting(0, 0, 0, 1); // start at point [0, 0] and have offset x = 0, and offset y = 1
            Weighting(1, 0, 0, 1);
            Weighting(2, 0, 0, 1);
            // Rows
            Weighting(0, 0, 1, 0);
            Weighting(0, 1, 1, 0);
            Weighting(0, 2, 1, 0);
            // Diagonals
            Weighting(0, 0, 1, 1);
            Weighting(0, 2, 1, -1);


            // Go through the weighted grid, look for the highest value, and place the corresponding X or O
            for (int x = 0; x <= 2; x++)
            {
                for (int y = 0; y <= 2; y++)
                {
                    if (HighestWeight <= WeightedGrid[x, y])
                    {
                        HighestWeight = WeightedGrid[x, y];
                        HighestX = x;
                        HighestY = y;
                    }
                }
            }

            PlaceMove(HighestX, HighestY, Crosses);

            // If its computer vs computer, chuck it back into checking the computer move
            if (ComputerGame)
            {
                CheckComputerMove();
            }
        }

        // If it is player1s go, and the computer is player 1, make a move (depending on difficulty) otherwise if it is player 2s go and the computer is player 2, make a move
        void CheckComputerMove()
        {
            // Only place a computer move if the game hasnt been won
            if (!ComputerGameWon && !NormalGameWon)
            {
                // If its noughts

                
                // If its noughts
                if (!P1sGo)
                {
                    if (Player2 == "EasyBot")
                    {
                        // Random
                        EasyBotMove();
                    }
                    else if (Player2 == "MediumBot")
                    {
                        // Defends
                        MediumBotMove(P1sGo);
                    }
                    else if (Player2 == "HardBot")
                    {
                        // As good as i could make it
                        HardBotMove(P1sGo);
                    }
                }

                // If its crosses
                else if (P1sGo)
                {
                    if (Player1 == "EasyBot")
                    {
                        // Random
                        EasyBotMove();
                    }
                    else if (Player1 == "MediumBot")
                    {
                        // Defends
                        MediumBotMove(P1sGo);
                    }
                    else if (Player1 == "HardBot")
                    {
                        // As good as i could make it
                        HardBotMove(P1sGo);
                    }
                }
                else
                {
                    P1sGo = true;
                    CheckComputerMove();
                }
    
            }

            // Reset label colours
            CorrectLabelColours();
        }

        void GameWon()
        {
            // Let them clear the grid
            btnNextRound.Visible = true;

            if (ComputerGame)
            {
                // If its a computer game, then stop them playing any further
                ComputerGameWon = true;
            }
            else
            {
                // Prevent player from playing any more pieces, and clear old labels
                btnPos1.Enabled = false; btnPos2.Enabled = false; btnPos3.Enabled = false;
                btnPos4.Enabled = false; btnPos5.Enabled = false; btnPos6.Enabled = false;
                btnPos7.Enabled = false; btnPos8.Enabled = false; btnPos9.Enabled = false;

                // If its a normal game (player vs computer), then stop them playing any further
                NormalGameWon = true;
            }
        }

        // Remove all aobjects from the screen, other than the timer and minimise/ exit button
        void ClearGUI()
        {
            foreach (Control GUI_Item in this.Controls)
            {
                GUI_Item.Visible = false;
            }

            timer1.Enabled = false;
            timer2.Enabled = false;
            btnExit.Visible = true;
            btnMinimise.Visible = true;
        }

        void CorrectLabelColours()
        {
            // Change colour of O or X to corresponding colour chosen before game begins
            if (P1sGo)
            {
                lblPlayer1Label2.ForeColor = lblPreview1.BackColor;
                lblPlayer1Label2.Font = new Font("Dubai", 30, FontStyle.Bold);

                lblPlayer2Label2.ForeColor = Color.Black;
                lblPlayer2Label2.Font = new Font("Dubai", 30, FontStyle.Regular);
            }
            else
            {
                lblPlayer2Label2.ForeColor = lblPreview2.BackColor;
                lblPlayer2Label2.Font = new Font("Dubai", 30, FontStyle.Bold);

                lblPlayer1Label2.ForeColor = Color.Black;
                lblPlayer1Label2.Font = new Font("Dubai", 30, FontStyle.Regular);
            }
        }

        // Reset variables in the grid
        void ClearGrid(int[,] GridToClear)
        {
            for (int Row = 0; Row <= 2; Row++)
            {
                for (int Column = 0; Column <= 2; Column++)
                {
                    GridToClear[Row, Column] = 0;
                }
            }
        }

        // Initialise the form and play the first song
        public Form1()
        {
            InitializeComponent();
            player.URL = "8 Bit Adventure.mp3";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Clear all objects on the screen and initialise correct GUI for home menu
            ClearGUI();
            
            lblGrid.Visible = true;
            btnStart.Visible = true;
            btnExit2.Visible = true;
            btnOptions.Visible = true;

            player.controls.play();
            player.settings.volume = 50;
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {
            // Clear all graphics and initialise correct GUI
            ClearGUI();

            lblVol.Visible = true;
            lblGrid.Visible = true;
            btnMenu.Visible = true;
            lblSong.Visible = true;
            clbSongs.Visible = true;
            lblVolume.Visible = true;
            tbrVolume.Visible = true;
            lblBackVol.Visible = true;
            lblBackSong.Visible = true;
            lblmenuBack.Visible = true;
            lblCoverBoxes.Visible = true;
            lblBackLabelMenuButton.Visible = true;

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Clear all graphics and initialise correct GUI
            ClearGUI();

            timer1.Enabled = true;

            lbxP1.Visible = true;
            lbxP2.Visible = true;
            lblP1.Visible = true;
            lblP2.Visible = true;
            btnBattle.Visible = true;
            lblP1Menu.Visible = true;
            lblP2Menu.Visible = true;
            lblPreview1.Visible = true;
            lblPreview2.Visible = true;
            lblP1Arrow1.Visible = true;
            lblP2Arrow1.Visible = true;
            btnRandomise1.Visible = true;
            btnRandomise2.Visible = true;
            BackLabelStartP1.Visible = true;
            BackLabelStartP2.Visible = true;
            BackLabel2StartP1.Visible = true;
            BackLabel2StartP2.Visible = true;
            lblBackLabelMenuButton.Visible = true;
            btnMenu.Visible = true; btnMenu.BringToFront();
            btnCyan.Visible = true; btnCyan2.Visible = true; btnBlue.Visible = true; btnBlue2.Visible = true; btnPurple.Visible = true; btnPurple2.Visible = true; btnPink.Visible = true; btnPink2.Visible = true;
            btnRed.Visible = true; btnRed2.Visible = true; btnOrange.Visible = true; btnOrange2.Visible = true; btnYellow.Visible = true; btnYellow2.Visible = true; btnLime.Visible = true; btnLime2.Visible = true;
        }

        private void btnBattle_Click(object sender, EventArgs e)
        {
            // Clear all graphics and initialise correct GUI
            ClearGUI();
            ClearGrid(Grid);

            timer2.Enabled = true;
            timer1.Enabled = false;
            btnMenu.Visible = true;
            lblGrid.Visible = false;
            lblP1Game.Visible = true;
            lblP2Game.Visible = true;
            lblPlayingGrid.Visible = true;
            lblGameP1Boarder.Visible = true;
            lblGameP2Boarder.Visible = true;
            lblBackLabelMenuButton.Visible = true;
            lblP1Score.Visible = true; lblP2Score.Visible = true;
            lblPlayer1Score.Visible = true; lblPlayer2Score.Visible = true;
            lblPlayer1Label1.Visible = true; lblPlayer2Label1.Visible = true;
            lblPlayer1Label2.Visible = true; lblPlayer2Label2.Visible = true;
            lblPos1.Visible = true; lblPos2.Visible = true; lblPos3.Visible = true;
            lblPos4.Visible = true; lblPos5.Visible = true; lblPos6.Visible = true;
            lblPos7.Visible = true; lblPos8.Visible = true; lblPos9.Visible = true;
            btnPos1.Enabled = true; btnPos2.Enabled = true; btnPos3.Enabled = true;
            btnPos4.Enabled = true; btnPos5.Enabled = true; btnPos6.Enabled = true;
            btnPos7.Enabled = true; btnPos8.Enabled = true; btnPos9.Enabled = true;
            btnPos1.BringToFront(); btnPos2.BringToFront(); btnPos3.BringToFront();
            btnPos4.BringToFront(); btnPos5.BringToFront(); btnPos6.BringToFront();
            btnPos7.BringToFront(); btnPos8.BringToFront(); btnPos9.BringToFront();

            // Reset winner after coming (back) in
            Winner = "";

            if (!ComputerGame)
            {
                lblComputerVsComputer.Visible = true;
            }
            else
            {
                btnNextRound.Visible = true;
            }

            // Checking the selected gamemode (decided by the selection index of the listbox)
            if (lbxP1.SelectedIndex == 0 && lbxP2.SelectedIndex == 0)
            {
                // Player VS Player

                Player1 = "User";
                Player2 = "User";
            }
            else if (lbxP1.SelectedIndex == 1 || lbxP2.SelectedIndex == 1)
            {
                // Player VS Bot: Easy

                if (lbxP1.SelectedIndex == 0)
                {
                    Player1 = "User";
                    Player2 = "EasyBot";
                }
                else if (lbxP2.SelectedIndex == 0)
                {
                    Player1 = "EasyBot";
                    Player2 = "User";
                }
                Console.WriteLine("Player vs easy");
            }
            else if (lbxP1.SelectedIndex == 2 || lbxP2.SelectedIndex == 2)
            {
                // Player VS Bot: Medium

                if (lbxP1.SelectedIndex == 0)
                {
                    Player1 = "User";
                    Player2 = "MediumBot";
                }
                else
                {
                    Player1 = "MediumBot";
                    Player2 = "User";
                }
            }
            else if (lbxP1.SelectedIndex == 3 || lbxP2.SelectedIndex == 3)
            {
                // Player VS Bot: Hard

                if (lbxP1.SelectedIndex == 0)
                {
                    Player1 = "User";
                    Player2 = "HardBot";
                }
                else
                {
                    Player1 = "HardBot";
                    Player2 = "User";
                }
            }
            else
            {
                // Player VS Player

                Player1 = "User";
                Player2 = "User";
            }

            // If its computer vs computer overwrite the players
            if (lbxP1.SelectedIndex != 0 && lbxP2.SelectedIndex != 0)
            {
                // Player 1
                if (lbxP1.SelectedIndex == 1)
                {
                    Player1 = "EasyBot";
                }
                else if (lbxP1.SelectedIndex == 2)
                {
                    Player1 = "MediumBot";
                }
                else if (lbxP1.SelectedIndex == 3)
                {
                    Player1 = "HardBot";
                }

                // Player 2
                if (lbxP2.SelectedIndex == 1)
                {
                    Player2 = "EasyBot";
                }
                else if (lbxP2.SelectedIndex == 2)
                {
                    Player2 = "MediumBot";
                }
                else if(lbxP2.SelectedIndex == 3)
                {
                    Player2 = "HardBot";
                }
            }


            // Change colour of score labels to correspoding colours chosen
            lblP1Score.ForeColor = lblPreview1.BackColor;
            lblP2Score.ForeColor = lblPreview2.BackColor;

            CorrectLabelColours();

            lblP1Score.Text = "0";
            lblP2Score.Text = "0";


            if (Player1 == "User")
            {
                // if there is a human playing first, then allow them to press the buttons
                btnPos1.Visible = true; btnPos2.Visible = true; btnPos3.Visible = true; btnPos4.Visible = true;
                btnPos5.Visible = true; btnPos6.Visible = true; btnPos7.Visible = true; btnPos8.Visible = true; btnPos9.Visible = true;

                ComputerGame = false;

                if(Player2 != "User")
                {
                    NormalGame = true; // normal game is player vs computer (or pc vs player)
                }
                else
                {
                    NormalGame = false;
                }
                
            }
            else if (Player2 == "User")
            {
                // Player 1 is not a user, so its a computer goes first game
                btnPos1.Visible = true; btnPos2.Visible = true; btnPos3.Visible = true; btnPos4.Visible = true;
                btnPos5.Visible = true; btnPos6.Visible = true; btnPos7.Visible = true; btnPos8.Visible = true; btnPos9.Visible = true;

                NormalGame = true;
                ComputerGame = false;
                CheckComputerMove();
            }
            else
            {
                NormalGame = false;

                // Computer vs computer
                ComputerGame = true;
                CheckComputerMove();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnExit2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tbrVolume_Scroll(object sender, EventArgs e)
        {
            // Change volume according to volume bar
            Volume = tbrVolume.Value;
            lblVolume.Text = Volume.ToString();
            player.settings.volume = Volume/2;
        }

        private void btnMenu_Click(object sender, EventArgs e)
        {
            // Clear screen
            ClearGUI();

            // Reset whos go it is
            P1sGo = true;

            // Bring buttons infront of labels
            btnPos1.BringToFront(); btnPos2.BringToFront(); btnPos3.BringToFront();
            btnPos4.BringToFront(); btnPos5.BringToFront(); btnPos6.BringToFront();
            btnPos7.BringToFront(); btnPos8.BringToFront(); btnPos9.BringToFront();
            ClearGrid(Grid);
            ClearGrid(WeightedGrid);

            // Bring up the menu buttons
            lblGrid.Visible = true;
            btnStart.Visible = true;
            btnExit2.Visible = true;
            btnOptions.Visible = true;

            NormalGame = false;
            ComputerGame = false;
            ComputerGameWon = false;
        }

        // Choice of selected music
        private void clbSongs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (clbSongs.SelectedIndex == 0)
            {
                player.URL = "Downforce.mp3";
            }
            else if (clbSongs.SelectedIndex == 1)
            {
                player.URL = "Night Shade.mp3";
            }
            else if (clbSongs.SelectedIndex == 2)
            {
                player.URL = "Pancakes.mp3";
            }
            else if (clbSongs.SelectedIndex == 3)
            {
                player.URL = "8 Bit Adventure.mp3";
            }

            player.controls.play();
        }

        // ------------ If the colour buttons are pressed to change the player's colour theme ------------

        private void btnRed_Click(object sender, EventArgs e)
        {
            lblPreview1.BackColor = Color.Red;
        }

        private void btnRed2_Click(object sender, EventArgs e)
        {
            lblPreview2.BackColor = Color.Red;
        }

        private void btnOrange_Click(object sender, EventArgs e)
        {
            lblPreview1.BackColor = Color.Orange;
        }

        private void btnOrange2_Click(object sender, EventArgs e)
        {
            lblPreview2.BackColor = Color.Orange;
        }

        private void btnYellow_Click(object sender, EventArgs e)
        {
            lblPreview1.BackColor = Color.Yellow;
        }

        private void btnYellow2_Click(object sender, EventArgs e)
        {
            lblPreview2.BackColor = Color.Yellow;
        }

        private void btnLime_Click(object sender, EventArgs e)
        {
            lblPreview1.BackColor = Color.Lime;
        }

        private void btnLime2_Click(object sender, EventArgs e)
        {
            lblPreview2.BackColor = Color.Lime;
        }

        private void btnCyan_Click(object sender, EventArgs e)
        {
            lblPreview1.BackColor = Color.Cyan;
        }

        private void btnCyan2_Click(object sender, EventArgs e)
        {
            lblPreview2.BackColor = Color.Cyan;
        }

        private void btnBlue_Click(object sender, EventArgs e)
        {
            lblPreview1.BackColor = Color.DodgerBlue;
        }

        private void btnBlue2_Click(object sender, EventArgs e)
        {
            lblPreview2.BackColor = Color.DodgerBlue;
        }

        private void btnPurple_Click(object sender, EventArgs e)
        {
            lblPreview1.BackColor = Color.Purple;
        }

        private void btnPurple2_Click(object sender, EventArgs e)
        {
            lblPreview2.BackColor = Color.Purple;
        }

        private void btnPink_Click(object sender, EventArgs e)
        {
            lblPreview1.BackColor = Color.Pink;
        }

        private void btnPink2_Click(object sender, EventArgs e)
        {
            lblPreview2.BackColor = Color.Pink;
        }

        private void btnRandomise1_Click(object sender, EventArgs e)
        {
            lblPreview1.BackColor = Color.FromArgb(Rand.Next(255), Rand.Next(255), Rand.Next(255));
        }

        private void btnRandomise2_Click(object sender, EventArgs e)
        {
            lblPreview2.BackColor = Color.FromArgb(Rand.Next(255), Rand.Next(255), Rand.Next(255));
        }

        // --- The following buttons are from the grid, in the following format: ---
        //  1  2  3
        //  4  5  6 
        //  7  8  9

        private void btnPos1_Click(object sender, EventArgs e)
        {
            if (P1sGo)
            {
                // Place a nought in slot 1 if its empty
                lblPos1.Image = Properties.Resources.O___2;
                Grid[0, 0] = 10;
            }
            else
            {
                // Else place a cross
                lblPos1.Image = Properties.Resources.X__2;
                Grid[0, 0] = 1;
            }
            // After they have been disable mouse hover and swap goes
            P1sGo = !P1sGo;
            btnPos1.Visible = false;
            CheckWin();
            CheckComputerMove();
        }

        private void btnPos2_Click(object sender, EventArgs e)
        {
            if (P1sGo)
            {
                // Place a nought in slot 2 if its empty
                lblPos2.Image = Properties.Resources.O___2;
                Grid[0, 1] = 10;
            }
            else
            {
                // Else place a cross
                lblPos2.Image = Properties.Resources.X__2;
                Grid[0, 1] = 1;
            }
            // After they have been disable mouse hover and swap goes
            P1sGo = !P1sGo;
            btnPos2.Visible = false;
            CheckWin();
            CheckComputerMove();
        }

        private void btnPos3_Click(object sender, EventArgs e)
        {
            if (P1sGo)
            {
                // Place a nought in slot 3 if its empty
                lblPos3.Image = Properties.Resources.O___2;
                Grid[0, 2] = 10;
            }
            else
            {
                // Else place a cross
                lblPos3.Image = Properties.Resources.X__2;
                Grid[0, 2] = 1;
            }
            // After they have been disable mouse hover and swap goes
            P1sGo = !P1sGo;
            btnPos3.Visible = false;
            CheckWin();
            CheckComputerMove();
        }

        private void btnPos4_Click(object sender, EventArgs e)
        {
            if (P1sGo)
            {
                // Place a nought in slot 4 if its empty
                lblPos4.Image = Properties.Resources.O___2;
                Grid[1, 0] = 10;
            }
            else
            {
                // Else place a cross
                lblPos4.Image = Properties.Resources.X__2;
                Grid[1, 0] = 1;
            }
            // After they have been disable mouse hover and swap goes
            P1sGo = !P1sGo;
            btnPos4.Visible = false;
            CheckWin();
            CheckComputerMove();
        }

        private void btnPos5_Click(object sender, EventArgs e)
        {
            if (P1sGo)
            {
                // Place a nought in slot 5 if its empty
                lblPos5.Image = Properties.Resources.O___2;
                Grid[1, 1] = 10;
            }
            else
            {
                // Else place a cross
                lblPos5.Image = Properties.Resources.X__2;
                Grid[1, 1] = 1;
            }
            // After they have been disable mouse hover and swap goes
            P1sGo = !P1sGo;
            btnPos5.Visible = false;
            CheckWin();
            CheckComputerMove();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (P1sGo)
            {
                // Place a nought in slot 6 if its empty
                lblPos6.Image = Properties.Resources.O___2;
                Grid[1, 2] = 10;
            }
            else
            {
                // Else place a cross
                lblPos6.Image = Properties.Resources.X__2;
                Grid[1, 2] = 1;
            }
            // After they have been disable mouse hover and swap goes
            P1sGo = !P1sGo;
            btnPos6.Visible = false;
            CheckWin();
            CheckComputerMove();
        }

        private void btnPos7_Click(object sender, EventArgs e)
        {
            if (P1sGo)
            {
                // Place a nought in slot 7 if its empty
                lblPos7.Image = Properties.Resources.O___2;
                Grid[2, 0] = 10;
            }
            else
            {
                // Else place a cross
                lblPos7.Image = Properties.Resources.X__2;
                Grid[2, 0] = 1;
            }
            // After they have been disable mouse hover and swap goes
            P1sGo = !P1sGo;
            btnPos7.Visible = false;
            CheckWin();
            CheckComputerMove();
        }

        private void btnNextRound_Click(object sender, EventArgs e)
        {
            ClearGrid(Grid);
            ClearGrid(WeightedGrid);

            CorrectLabelColours();

            if (ComputerGame)
            {
                // And clear the old labels
                lblPos1.Image = null; lblPos2.Image = null; lblPos3.Image = null;
                lblPos4.Image = null; lblPos5.Image = null; lblPos6.Image = null;
                lblPos7.Image = null; lblPos8.Image = null; lblPos9.Image = null;
                // If its computer vs computer, chuck it back into checking the computer move
                ComputerGameWon = false;
                CheckComputerMove();
            }
            else
            {
                // If its not a computer vs computer game, then make buttons visible and enable them
                btnPos1.Visible = true; btnPos2.Visible = true; btnPos3.Visible = true;
                btnPos4.Visible = true; btnPos5.Visible = true; btnPos6.Visible = true;
                btnPos7.Visible = true; btnPos8.Visible = true; btnPos9.Visible = true;

                btnPos1.Enabled = true; btnPos2.Enabled = true; btnPos3.Enabled = true;
                btnPos4.Enabled = true; btnPos5.Enabled = true; btnPos6.Enabled = true;
                btnPos7.Enabled = true; btnPos8.Enabled = true; btnPos9.Enabled = true;

                NormalGameWon = false;

                // then make the button invisible again if its a player game
                btnNextRound.Visible = false;
            }

            Console.WriteLine("Is player vs computer" + NormalGame);

            if (NormalGame)
            {
                Console.WriteLine("Checking for computer going first");
                if (Winner == "Noughts" && Player2 != "User")             // As loser always goes first, if the winner is player 1, and player 2 is a bot...
                {
                    CheckComputerMove();                                  // Play computer move
                }
                else if (Winner == "Crosses" && Player1 != "User")
                {
                    CheckComputerMove();
                }
                else if (Winner == "Draw" && P1sGo && Player1 != "User")  // If the last game was a draw, then see if the computer is meant to move by looking at 'P1sGo' bool
                {
                    CheckComputerMove();
                }
                else if (Winner == "Draw" && !P1sGo && Player2 != "User") // Same for if the bot was player 2 but invert p1s go
                {
                    CheckComputerMove();
                }
            }

            NormalGameWon = false;
            ComputerGameWon = false;
        }

        private void btnPos8_Click(object sender, EventArgs e)
        {
            if (P1sGo)
            {
                // Place a nought in slot 8 if its empty
                lblPos8.Image = Properties.Resources.O___2;
                Grid[2, 1] = 10;
            }
            else
            {
                // Else place a cross
                lblPos8.Image = Properties.Resources.X__2;
                Grid[2, 1] = 1;
            }
            // After they have been disable mouse hover and swap goes
            P1sGo = !P1sGo;
            btnPos8.Visible = false;
            CheckWin();
            CheckComputerMove();
        }

        private void btnPos9_Click(object sender, EventArgs e)
        {
            if (P1sGo)
            {
                // Place a nought in slot 9 if its empty
                lblPos9.Image = Properties.Resources.O___2;
                Grid[2, 2] = 10;
            }
            else
            {
                // Else place a cross
                lblPos9.Image = Properties.Resources.X__2;
                Grid[2, 2] = 1;
            }
            // After they have been disable mouse hover and swap goes
            P1sGo = !P1sGo;
            btnPos9.Visible = false;
            CheckWin();
            CheckComputerMove();
        }

        // If the selection box index is changed show the little black arrow to the box selected
        private void lbxP1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblP1Arrow1.Visible = false;
            lblP1Arrow2.Visible = false;
            lblP1Arrow3.Visible = false;
            lblP1Arrow4.Visible = false;

            if (lbxP1.SelectedIndex == 0)
            {
                lblP1Arrow1.Visible = true;
            }
            else if (lbxP1.SelectedIndex == 1)
            {
                lblP1Arrow2.Visible = true;
            }
            else if (lbxP1.SelectedIndex == 2)
            {
                lblP1Arrow3.Visible = true;
            }
            else
            {
                lblP1Arrow4.Visible = true;
            }
        }

        // If the selection box index is changed show the little black arrow to the box selected
        private void lbxP2_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblP2Arrow1.Visible = false;
            lblP2Arrow2.Visible = false;
            lblP2Arrow3.Visible = false;
            lblP2Arrow4.Visible = false;

            if (lbxP2.SelectedIndex == 0)
            {
                lblP2Arrow1.Visible = true;
            }
            else if (lbxP2.SelectedIndex == 1)
            {
                lblP2Arrow2.Visible = true;
            }
            else if (lbxP2.SelectedIndex == 2)
            {
                lblP2Arrow3.Visible = true;
            }
            else
            {
                lblP2Arrow4.Visible = true;
            }
        }

        
        // --- Animation for character for the player 1 and player 2 ---
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (ImageP1 < 5)
            {
                ImageP1++;
            }
            else
            {
                ImageP1 = 0;
            }
            lblP1Menu.Image = ImagesP1[ImageP1];

            if (ImageP2 < 3)
            {
                ImageP2++;
            }
            else
            {
                ImageP2 = 0;
            }
            lblP2Menu.Image = ImagesP2[ImageP2];
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (ImageP1 < 5)
            {
                ImageP1++;
            }
            else
            {
                ImageP1 = 0;
            }
            lblP1Game.Image = ImagesP1[ImageP1];

            if (ImageP2 < 3)
            {
                ImageP2++;
            }
            else
            {
                ImageP2 = 0;
            }
            lblP2Game.Image = ImagesP2[ImageP2];
        }

        // If the following lines are deleted it will cause the program to error
        private void lblBackVol_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void lblPos8_Click(object sender, EventArgs e) { }

    }
}
