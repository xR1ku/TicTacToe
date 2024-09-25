using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerOption
{
    NONE, //0
    X, // 1
    O // 2
}

public class TTT : MonoBehaviour
{
    public int Rows;
    public int Columns;
    [SerializeField] BoardView board;

    PlayerOption currentPlayer = PlayerOption.X;
    Cell[,] cells;

    // Start is called before the first frame update
    void Start()
    {
        cells = new Cell[Columns, Rows];

        board.InitializeBoard(Columns, Rows);

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                cells[j, i] = new Cell();
                cells[j, i].current = PlayerOption.NONE;
            }
        }
    }

    public void MakeOptimalMove()
    {
        Debug.Log($"MakeOptimalMove called for player: {currentPlayer}");

        // Check if AI can win in the next move
        if (TryMakeWinningMove())
        {
            Debug.Log("Winning move made");
            return;
        }

        // Check if AI needs to block the opponent from winning
        if (TryBlockOpponentWin())
        {
            Debug.Log("Blocked opponent's winning move");
            return;
        }

        // Choose the center if available
        if (IsCellAvailable(1, 1))
        {
            Debug.Log("Took center position");
            MakeMove(1, 1);
            return;
        }

        // Choose a corner if available
        if (TryTakeCorner())
        {
            Debug.Log("Took a corner");
            return;
        }

        // Choose any remaining available space
        MakeAnyMove();
        Debug.Log("Made any available move");
    }

    // Helper method to check if a cell is available
    private bool IsCellAvailable(int column, int row)
    {
        return cells[column, row].current == PlayerOption.NONE;
    }

    private bool TryMakeWinningMove()
    {
        return TryCompleteLine(currentPlayer);
    }

    private bool TryBlockOpponentWin()
    {
        PlayerOption opponent = currentPlayer == PlayerOption.X ? PlayerOption.O : PlayerOption.X;
        return TryCompleteLine(opponent);
    }

    private bool TryCompleteLine(PlayerOption player)
    {
        // Try to complete a line (row, column, or diagonal) for the given player
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (cells[j, i].current == PlayerOption.NONE)
                {
                    // Make a temporary move
                    cells[j, i].current = player;
                    if (GetWinner() == player)
                    {
                        // If this move results in a win, finalize the move
                        MakeMove(j, i);
                        cells[j, i].current = PlayerOption.NONE; // Reset after checking
                        return true;
                    }
                    // Reset after checking
                    cells[j, i].current = PlayerOption.NONE;
                }
            }
        }
        return false;
    }

    private bool TryTakeCorner()
    {
        if (IsCellAvailable(0, 0))
        {
            MakeMove(0, 0);
            return true;
        }
        if (IsCellAvailable(0, 2))
        {
            MakeMove(0, 2);
            return true;
        }
        if (IsCellAvailable(2, 0))
        {
            MakeMove(2, 0);
            return true;
        }
        if (IsCellAvailable(2, 2))
        {
            MakeMove(2, 2);
            return true;
        }
        return false;
    }

    private void MakeAnyMove()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (IsCellAvailable(j, i))
                {
                    MakeMove(j, i);
                    return;
                }
            }
        }
        Debug.Log("No available move found");
    }

    private void MakeMove(int column, int row)
    {
        if (!IsCellAvailable(column, row))
        {
            Debug.LogWarning($"Cell ({column},{row}) is not available! Skipping move.");
            return;
        }

        Debug.Log($"Player {currentPlayer} makes a move at ({column},{row})");
        cells[column, row].current = currentPlayer;
        board.UpdateCellVisual(column, row, currentPlayer);
        EndTurn();
    }

    public void ChooseSpace(int column, int row)
    {
        // can't choose space if game is over
        if (GetWinner() != PlayerOption.NONE)
            return;

        // can't choose a space that's already taken
        if (cells[column, row].current != PlayerOption.NONE)
            return;

        // set the cell to the player's mark
        cells[column, row].current = currentPlayer;

        // update the visual to display X or O
        board.UpdateCellVisual(column, row, currentPlayer);

        // if there's no winner, keep playing, otherwise end the game
        if (GetWinner() == PlayerOption.NONE)
            EndTurn();
        else
        {
            Debug.Log("GAME OVER!");
        }
    }

    public void EndTurn()
    {
        // increment player, if it goes over player 2, loop back to player 1
        currentPlayer += 1;
        if ((int)currentPlayer > 2)
            currentPlayer = PlayerOption.X;

        Debug.Log($"Next turn: {currentPlayer}");
    }

    public PlayerOption GetWinner()
    {
        // sum each row/column based on what's in each cell X = 1, O = -1, blank = 0
        // we have a winner if the sum = 3 (X) or -3 (O)
        int sum = 0;

        // check rows
        for (int i = 0; i < Rows; i++)
        {
            sum = 0;
            for (int j = 0; j < Columns; j++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check columns
        for (int j = 0; j < Columns; j++)
        {
            sum = 0;
            for (int i = 0; i < Rows; i++)
            {
                var value = 0;
                if (cells[j, i].current == PlayerOption.X)
                    value = 1;
                else if (cells[j, i].current == PlayerOption.O)
                    value = -1;

                sum += value;
            }

            if (sum == 3)
                return PlayerOption.X;
            else if (sum == -3)
                return PlayerOption.O;

        }

        // check diagonals
        // top left to bottom right
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            int value = 0;
            if (cells[i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        // top right to bottom left
        sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            int value = 0;

            if (cells[Columns - 1 - i, i].current == PlayerOption.X)
                value = 1;
            else if (cells[Columns - 1 - i, i].current == PlayerOption.O)
                value = -1;

            sum += value;
        }

        if (sum == 3)
            return PlayerOption.X;
        else if (sum == -3)
            return PlayerOption.O;

        return PlayerOption.NONE;
    }
}
