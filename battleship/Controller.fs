module Controller
open Model
open View
open GameCore

let initialModel = {
    player = { ships = randomPlacement (); shots = [] }
    ai = { ships = randomPlacement (); shots = [] }
    state = PlayerTurn
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
            let newTile = { x = tx; y = ty }
            if List.contains newTile model.player.shots then model
            else 
                let newPlayer = { model.player with shots = newTile::model.player.shots }
                { model with player = newPlayer; state = AITurn 0. }


let advanceGame runState gameModel = 
    match gameModel with
    | Some model -> 
        checkForShot runState model |> Some
    | None -> Some initialModel