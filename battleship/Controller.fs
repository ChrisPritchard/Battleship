module Controller
open Model
open View
open GameCore

let initialModel = {
    player = { ships = []; shots = [] }
    ai = { ships = randomPlacement (); shots = [] }
    state = Title
}
let timeBetweenAIActions = 1000.

let checkForStart runState model = 
    let (pressed,_) = runState.mouse.pressed
    if not pressed then model
    else
        let (bx,by,bw,bh) = startButton
        let (mx,my) = runState.mouse.position
        if mx >= bx && mx < bx+bw && my >= by && my < by+bh then
            { model with state = Placement (Dir.North,shipList) }
        else model

let checkForPlacement (runState: RunState) model (dir,remaining) = 
    match remaining with
    | [] -> { model with state = PlayerTurn }
    | head::tail ->
        let (left,right) = runState.mouse.pressed
        let dir = if right then enum<Dir>((int dir + 1) % 4) else dir
        match findMouseTile runState placementOffset placementTilesize with
        | Some tile when left ->
            let tiles = tilesIn tile dir head
            if canPlace tiles model.player.ships && List.length tiles = head then
                let newShip = nameForLength head, tiles
                { model with 
                    player = { model.player with ships = newShip::model.player.ships } 
                    state = Placement (dir,tail) }
            else
                { model with state = Placement (dir,remaining) }
        | _ -> { model with state = Placement (dir,remaining) }

let hasWon ships shots =
    ships 
        |> List.collect (fun (_,t) -> t)
        |> List.except shots
        |> List.isEmpty

let checkForShot (runState: RunState) model = 
    let (pressed,_) = runState.mouse.pressed
    if not pressed then model
    else
        match findMouseTile runState playerShotsOffset playerShotsTileSize with
        | None -> model
        | Some tile ->
            if List.contains tile model.player.shots then model
            else 
                let newPlayer = { model.player with shots = tile::model.player.shots }
                let newState = 
                    if hasWon model.ai.ships newPlayer.shots
                    then GameOver true else AITurn (runState.elapsed,false)
                { model with player = newPlayer; state = newState }

let advanceAi model runState hasActed =
    let rec takeShot () =
        let tile = { x = random.Next (0, boardx); y = random.Next (0, boardy) } 
        if List.contains tile model.ai.shots then takeShot () else tile
    if hasActed then
        let newState = 
            if hasWon model.player.ships model.ai.shots
            then GameOver false else PlayerTurn
        { model with state = newState }
    else 
        let newAi = { model.ai with shots = takeShot()::model.ai.shots }
        { model with ai = newAi; state = AITurn (runState.elapsed,true) }

let checkForRestart runState model = 
    let (pressed,_) = runState.mouse.pressed
    if not pressed then model
    else
        let (bx,by,bw,bh) = restartButton
        let (mx,my) = runState.mouse.position
        if mx >= bx && mx < bx+bw && my >= by && my < by+bh 
        then initialModel else model

let advanceGame runState gameModel = 
    match gameModel with
    | Some model ->
        let newState =
            match model.state with
            | Title -> checkForStart runState model
            | Placement (dir,rem) -> checkForPlacement runState model (dir,rem)
            | PlayerTurn -> checkForShot runState model
            | AITurn (last,acted) when runState.elapsed - last > timeBetweenAIActions -> advanceAi model runState acted
            | GameOver _ -> checkForRestart runState model
            | _ -> model
        Some newState
    | None -> Some initialModel