module View
open GameCore
open Model
open Microsoft.Xna.Framework

let (tilew,tileh) = 50,50
let (offsetx,offsety) = 50,50

let resolution = Windowed ((offsetx*2)+(tilew*boardx),(offsety*2)+(tileh*boardy))

let assetsToLoad = [
    Texture { key = "blank"; path = "Content/white" }
    Texture { key = "cursor"; path = "Content/cursor" }
    Font { key = "default"; path = "Content/miramo" }
]

let boardTiles = 
    let boardSize = offsetx, offsety, tilew*boardx, tileh*boardy
    let back = ColouredImage (Color.Black, { assetKey = "blank"; destRect = boardSize; sourceRect = None })
    let sea = 
        [0..boardx-1] |> List.collect (fun x -> [0..boardy-1] |> List.map (fun y -> 
            let rect = offsetx+x*tilew+1, offsety+y*tileh+1, tilew-2, tileh-2
            ColouredImage (Color.Blue, { assetKey = "blank"; destRect = rect; sourceRect = None })))
    back::sea

let shipTiles (shipName,tiles) = 
    let label = Seq.head shipName |> string
    tiles |> List.collect(fun t -> 
        let pos = offsetx+t.x*tilew+3+tilew/2,offsety+t.y*tileh+3+tileh/2
        let rect = offsetx+t.x*tilew+3,offsety+t.y*tileh+3,tilew-6,tileh-6
        [
            ColouredImage (Color.Gray, { assetKey = "blank"; destRect = rect; sourceRect = None })
            ColouredText (Color.White, { assetKey = "default"; text = label; position = pos; origin = Centre; scale = 0.2 })
        ])

let getView runState model = 
    let currentPlayer = if model.isFirstPlayerTurn then fst model.players else snd model.players
    let shipTiles = currentPlayer.ships |> List.collect shipTiles
            
    let (mx,my) = runState.mouse.position
    let cursor = Image { assetKey = "cursor"; destRect = mx-16,my-16,32,32; sourceRect = None }

    boardTiles @ shipTiles @ [cursor]