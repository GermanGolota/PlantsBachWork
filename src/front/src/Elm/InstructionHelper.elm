module InstructionHelper exposing (..)

import Endpoints exposing (Endpoint(..), getAuthed)
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
    , familyId : String
    }


getInstruction : (Result Http.Error (Maybe InstructionView) -> msg) -> String -> String -> Cmd msg
getInstruction cmd token id =
    let
        expect =
            Http.expectJson cmd decodeInstruction
    in
    getAuthed token (GetInstruction id) expect Nothing


decodeInstruction : D.Decoder (Maybe InstructionView)
decodeInstruction =
    existsDecoder decodeInstructionBase


decodeInstructionBase : D.Decoder InstructionView
decodeInstructionBase =
    let
        requiredItem name =
            requiredAt [ "item", name ]
    in
    D.succeed InstructionView
        |> requiredItem "id" decodeId
        |> requiredItem "title" D.string
        |> requiredItem "description" D.string
        |> custom (D.at [ "item", "coverUrl" ] coverDecoder)
        |> requiredItem "instructionText" D.string
        |> requiredItem "plantFamilyName" decodeId


coverDecoder : D.Decoder (Maybe String)
coverDecoder =
    D.nullable D.string |> D.andThen coverImageDecoder


coverImageDecoder url =
    case url of
        Just loc ->
            D.succeed <| Just loc

        Nothing ->
            D.succeed Nothing
