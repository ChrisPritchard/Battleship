module Controller
open Model
open View
open GameCore

let initialModel = {
    players = 
        { 
            ships = randomPlacement (); shots = [] 
        }, 
        { 
            ships = randomPlacement (); shots = [] 
        }
    isFirstPlayerTurn = true
}

let checkForShot (runState: RunState) model = 
    let (pressed,_) = runState.mouse.pressed
    if not pressed then model
    else
        let (mx,my) = runState.mouse.position
        let (rawx,rawy) = (float mx-float offsetx / float tilew, float my-float offsety / float tileh)
        let (tx,ty) = (floor rawx |> int, floor rawy |> int)
        if tx < 0 || tx >= boardx || ty < 0 || ty >= boardy then
            model
        else
            let currentPlayer = if model.isFirstPlayerTurn then fst model.players else snd model.players
            let newTile = { x = tx; y = ty }
            if List.contains newTile currentPlayer.shots then model
            else 
                let newPlayer = { currentPlayer with shots = newTile::currentPlayer.shots }
                if model.isFirstPlayerTurn then 
                    { model with players = (newPlayer, snd model.players); isFirstPlayerTurn = not model.isFirstPlayerTurn }
                else 
                    { model with players = (fst model.players, newPlayer); isFirstPlayerTurn = not model.isFirstPlayerTurn }


let advanceGame runState gameModel = 
    match gameModel with
    | Some model -> 
        checkForShot runState model |> Some
    | None -> Some initialModel