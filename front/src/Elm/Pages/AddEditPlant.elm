module Pages.AddEditPlant exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Form.Input as Input
import Bootstrap.Form.Select as Select
import Bootstrap.Utilities.Flex as Flex
import Dict
import Endpoints exposing (Endpoint(..), getAuthed)
import File exposing (File)
import File.Select as FileSelect
import Html exposing (Html, div, text)
import Html.Attributes exposing (value)
import Http
import ImageList
import Json.Decode as D
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase)
import Multiselect
import NavBar exposing (plantsLink, viewNav)
import Pages.Search exposing (Available, availableDecoder)
import Utils exposing (fillParent, flex, flex1, largeCentered, smallMargin)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = Add (WebData Available) PlantView
    | Edit (WebData Available) Int (WebData PlantView)
    | BadEdit


type alias PlantView =
    { name : String
    , description : String
    , created : String
    , regions : Multiselect.Model
    , soil : Int
    , group : Int
    , images : ImageList.Model
    }



--update


type Msg
    = NoOp
    | Images ImageList.Msg
    | NameUpdate String
    | DescriptionUpdate String
    | SoilUpdate Int
    | GroupUpdate Int
    | DateUpdate String
    | StartUpload
    | ImagesLoaded File (List File)
    | RegionsMS Multiselect.Msg
    | GotAvailable (Result Http.Error Available)


update : Msg -> Model -> ( Model, Cmd Msg )
update msg m =
    let
        noOp =
            ( m, Cmd.none )
    in
    case m of
        Authorized auth model ->
            let
                authed =
                    Authorized auth
            in
            case ( msg, model ) of
                ( StartUpload, _ ) ->
                    ( m, requestImages )

                ( GotAvailable (Ok res), Edit _ id plantView ) ->
                    ( authed <| Edit (Loaded res) id plantView, Cmd.none )

                ( GotAvailable (Err err), Edit _ id plantView ) ->
                    ( authed <| Edit Error id plantView, Cmd.none )

                ( GotAvailable (Ok res), Add _ plantView ) ->
                    ( authed <| Add (Loaded res) { plantView | regions = res.regions }, Cmd.none )

                ( GotAvailable (Err err), Add _ plantView ) ->
                    ( authed <| Add Error plantView, Cmd.none )

                ( RegionsMS msEvent, Add av plantView ) ->
                    let
                        ( subModel, subCmd, _ ) =
                            Multiselect.update msEvent plantView.regions
                    in
                    ( authed <| Add av { plantView | regions = subModel }, Cmd.map RegionsMS subCmd )

                ( _, _ ) ->
                    noOp

        _ ->
            noOp



--commands
--view


view : Model -> Html Msg
view model =
    viewNav model (Just plantsLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    case page of
        BadEdit ->
            div [] [ text "There is no such plant" ]

        Edit av id plant ->
            viewWebdata plant (viewPlant av True)

        Add av plant ->
            viewPlant av False plant


viewPlant av isEdit plant =
    viewWebdata av (viewPlantBase isEdit plant)


viewPlantBase isEdit plant av =
    let
        btnMsg =
            if isEdit then
                "Edit"

            else
                "Add"

        btnEvent =
            NoOp

        viewOption ( val, desc ) =
            Select.item [ value val ] [ text desc ]

        pareOrNoOp ev str =
            case String.toInt str of
                Just num ->
                    ev num

                Nothing ->
                    NoOp

        viewOptions vals =
            List.map viewOption (Multiselect.getValues vals)
    in
    div ([ flex, Flex.row ] ++ fillParent)
        [ div [ Flex.col, flex1, flex ]
            (viewInput "Name" (Input.text [ Input.onInput NameUpdate, Input.value plant.name ])
                ++ viewInput "Add Image" (Button.button [ Button.primary, Button.onClick StartUpload ] [ text "Upload" ])
                ++ viewInput "Regions" (Html.map RegionsMS <| Multiselect.view plant.regions)
                ++ viewInput "Soil"
                    (Select.select [ Select.onChange (pareOrNoOp SoilUpdate) ]
                        (viewOptions av.soils)
                    )
                ++ viewInput "Group"
                    (Select.select [ Select.onChange (pareOrNoOp GroupUpdate) ]
                        (viewOptions av.groups)
                    )
                ++ viewInput "Description" (Input.text [ Input.onInput DescriptionUpdate, Input.value plant.description ])
                ++ viewInput "Create Date" (Input.date [ Input.onInput DateUpdate, Input.value plant.created ])
            )
        , div [ flex, Flex.col, flex1, Flex.justifyBetween, Flex.alignItemsCenter ]
            [ div [ flex1 ] [ Html.map Images (ImageList.view plant.images) ]
            , div [ flex1 ]
                [ Button.button
                    [ Button.primary
                    , Button.onClick btnEvent
                    , Button.attrs [ smallMargin ]
                    ]
                    [ text btnMsg ]
                ]
            ]
        ]


viewInput : String -> Html msg -> List (Html msg)
viewInput desc input =
    [ div (largeCentered ++ [ flex1 ]) [ text desc ], div [ flex1 ] [ input ] ]


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        initial =
            decodeInitial flags

        initialCmd res =
            case initial of
                BadEdit ->
                    Cmd.none

                Add _ _ ->
                    getAvailable res.token

                _ ->
                    Cmd.none
    in
    initBase [ Producer, Consumer, Manager ] initial initialCmd resp


decodeInitial flags =
    let
        isEdit =
            Result.withDefault False <| D.decodeValue (D.field "isEdit" D.bool) flags
    in
    if isEdit then
        case D.decodeValue (D.field "plantId" D.int) flags of
            Ok id ->
                Edit Loading id Loading

            Err _ ->
                BadEdit

    else
        Add Loading (PlantView "" "" "" (Multiselect.initModel [] "regions" Multiselect.Show) 0 0 (ImageList.fromDict (Dict.fromList [])))


subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none


requestImages : Cmd Msg
requestImages =
    FileSelect.files [ "image/png", "image/jpg" ] ImagesLoaded


getAvailable : String -> Cmd Msg
getAvailable token =
    Endpoints.getAuthed token Dicts (Http.expectJson GotAvailable availableDecoder) Nothing


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
