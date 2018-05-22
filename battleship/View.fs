module View
open GameCore
open Model
open Microsoft.Xna.Framework

let (tilew,tileh) = 50,50
let (offsetx,offsety) = 50,50

let resolution = Windowed ((offsetx*2)+(tilew*boardx),(offsety*2)+(tileh*boardy))

let assetsToLoad = [
    Texture { key = "default"; path = "Content/white" }
]

let getView runState model = 
    let currentPlayer = if model.isFirstPlayerTurn then fst model.players else snd model.players
    let shipTiles = 
        currentPlayer.ships |> List.concat |> List.map (fun t ->
            let rect = offsetx+t.x*tilew+3,offsety+t.y*tileh+3,tilew-6,tileh-6
            ColouredImage (Color.Gray, { assetKey = "default"; destRect = rect; sourceRect = None })) 

    [ColouredImage (Color.Black, { assetKey = "default"; destRect = offsetx,offsety,tilew*boardx,tileh*boardy; sourceRect = None })]
    @ ([0..boardx-1] |> List.collect (fun x -> [0..boardy-1] |> List.map (fun y -> 
        let rect = offsetx+x*tilew+1, offsety+y*tileh+1,tilew-2,tileh-2
        ColouredImage (Color.Blue, { assetKey = "default"; destRect = rect; sourceRect = None }))))
    @ shipTiles