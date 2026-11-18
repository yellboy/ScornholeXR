# ScornholeXR Rules

ScornholeXR combines ranked-answer guessing with XR bag throwing. These rules describe the current game implementation.

For installation and multiplayer setup, see the [README](README.md).

## Objective

Score more total points than the other player by choosing high-value answers and throwing their bags onto the board or into the hole.

Questions ask for the **least popular, least liked, least ordered**, or a similar least-favored choice. There are five options, not one correct answer: every option has a different point value.

## Match setup

- A match requires **two players**.
- The host is **Player 1**; the joining player is **Player 2**.
- Player 1 chooses the match settings before starting:

| Setting | Default | Allowed range |
| --- | --- | --- |
| Rounds per match | 5 | 1-10 |
| Throws per player per round | 5 | 1-10 |

The match begins when both players are connected and the first question has loaded.

## Playing a round

1. Both players receive the same question and five numbered answers.
2. **Player 1 throws first in every round.**
3. On your turn, grab the bag corresponding to your chosen answer number and throw it toward the board.
4. After the game registers the result, the turn passes to the other player.
5. Continue alternating until both players have used their allotted throws.

Only the active player can grab available bags. A replacement bag becomes available after a scored throw, so **you may choose the same answer more than once**. Choosing an answer does not reserve it or prevent the other player from using it.

A miss still uses one throw.

## Scoring

The generated answer order runs from least to most popular. The least popular option is worth the most points.

| Option's rank | Board hit | Hole hit | Miss |
| --- | --- | --- | --- |
| 1st: least popular | 5 | 7 | 0 |
| 2nd | 4 | 6 | 0 |
| 3rd | 3 | 5 | 0 |
| 4th | 2 | 4 | 0 |
| 5th: most popular | 1 | 3 | 0 |

- **Board hit:** earn the chosen option's value.
- **Hole hit:** earn the option's value **plus 2 points**.
- **Miss:** earn **0 points**, regardless of the option.

The answer's displayed number is not its rank. For example, answer **4** could be the least popular answer and therefore be worth **5 points on the board** or **7 in the hole**.

### Example

Suppose the answer order is **4, 2, 3, 5, 1**, from least to most popular:

- Throwing bag **4** onto the board earns **5 points**.
- Throwing bag **2** into the hole earns **6 points**: 4 for the answer and 2 for the hole.
- Missing with bag **4** earns **0 points**.

Points are added independently for each player. There is **no cancellation scoring**, no deduction for an opponent's score, and no "first to 21" rule.

## Round results

After both players finish their throws:

1. Each player's round score is added to their match total.
2. The answer ranking and round scores are revealed.
3. Thrown bags are cleared.
4. If rounds remain, the results stay on screen for approximately **5 seconds**, then the next question is requested.

The scoreboard shows whose turn it is, throws used, round scores, and accumulated totals.

## Winning

After the configured final round:

- The player with the higher total wins.
- Equal totals produce a **tie**. There is no sudden-death round.

There is currently no in-game rematch button; restart the clients to begin another match.

## Current collision behavior

Scoring uses the game's registered collision result, not a bag's final resting position:

- The bag resolves on the physics update after a scoring contact. If multiple contact flags are present, hole takes priority over board, then floor.
- Once resolved, a throw is not rescored if the bag later moves or falls.
- Under the current implementation, hitting an already-resolved bag with a board-hit or hole-hit result counts as a hole hit for the incoming bag. Hitting a bag with a miss result counts as a miss.

These are implementation-specific behaviors, not standard cornhole rules.

## About the questions

Questions and rankings are AI-generated. The prompt asks for real sources, but the game does not verify the rankings or source claims. The generated ranking is the one used for scoring that round.
