module InstructionHelper exposing (..)

import Endpoints exposing (Endpoint(..), getAuthed, getImageUrl)
import Http
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, requiredAt)
import Utils exposing (decodeId, existsDecoder)


type alias InstructionView =
    { id : String
    , title : String
    , description : String
    , imageUrl : Maybe String
    , text : String
    , groupId : String
    }


getInstruction : (Result Http.Error (Maybe InstructionView) -> msg) -> String -> String -> Cmd msg
getInstruction cmd token id =
    let
        expect =
            Http.expectJson cmd (decodeInstruction token)
    in
    getAuthed token (GetInstruction id) expect Nothing


decodeInstruction : String -> D.Decoder (Maybe InstructionView)
decodeInstruction token =
    existsDecoder (decodeInstructionBase token)


decodeInstructionBase : String -> D.Decoder InstructionView
decodeInstructionBase token =
    let
        requiredItem name =
            requiredAt [ "item", name ]
    in
    D.succeed InstructionView
        |> requiredItem "id" decodeId
        |> requiredItem "title" D.string
        |> requiredItem "description" D.string
        |> custom (D.at [ "item", "coverUrl" ] <| coverDecoder token)
        |> requiredItem "instructionText" D.string
        |> requiredItem "plantGroupName" decodeId


coverDecoder : String -> D.Decoder (Maybe String)
coverDecoder token =
    D.nullable D.string |> D.andThen (coverImageDecoder token)


coverImageDecoder token url =
    case url of
        Just loc ->
            D.succeed <| Just <| getImageUrl token loc

        Nothing ->
            D.succeed Nothing
