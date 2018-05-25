module Controller
open Model
open View
open GameCore

let initialModel = {
    player = { ships = randomPlacement (); shots = [] }
    ai = { ships = randomPlacement (); shots = [] }
    state = PlayerTurn
}
let timeBetweenAIActions = 1000.

let checkForStart runState model = model

let checkForPlacement runState model remaining = model

let checkForShot (runState: RunState) model = 
    let (pressed,_) = runState.mouse.pressed
    if not pressed then model
    else
        let (mx,my) = runState.mouse.position
        let (offsetx,offsety) = playerShotsOffset
        let (tilew,tileh) = playerShotsTileSize
        let (rawx,rawy) = mx - offsetx, my - offsety
        let (tx,ty) = floor (float rawx / float tilew) |> int, floor (float rawy / float tileh) |> int
        if tx < 0 || tx >= boardx || ty < 0 || ty >= boardy then
            model
        else
            let newTile = { x = tx; y = ty }
            if List.contains newTile model.player.shots then model
            else 
                let newPlayer = { model.player with shots = newTile::model.player.shots }
                { model with player = newPlayer; state = AITurn (runState.elapsed,false) }

let advanceAi model runState hasActed =
    let rec takeShot () =
        let tile = { x = random.Next (0, boardx); y = random.Next (0, boardy) } 
        if List.contains tile model.ai.shots then takeShot () else tile
    if hasActed then { model with state = PlayerTurn }
    else 
        let newAi = { model.ai with shots = takeShot()::model.ai.shots }
        { model with ai = newAi; state = AITurn (runState.elapsed,true) }

let checkForRestart runState model = model

let advanceGame runState gameModel = 
    match gameModel with
    | Some model ->
        let newState =
            match model.state with
            | Title -> checkForStart runState model
            | Placement rem -> checkForPlacement runState model rem
            | PlayerTurn -> checkForShot runState model
            | AITurn (last,acted) when runState.elapsed - last > timeBetweenAIActions -> advanceAi model runState acted
            | GameOver -> checkForRestart runState model
            | _ -> model
        Some newState
    | None -> Some initialModel