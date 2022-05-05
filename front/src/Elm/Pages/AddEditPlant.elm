module Pages.AddEditPlant exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Form.Input as Input
import Bootstrap.Form.Select as Select
import Bootstrap.Utilities.Flex as Flex
import Dict
import Endpoints exposing (Endpoint(..), getAuthed, imagesDecoder, postAuthed)
import File exposing (File)
import File.Select as FileSelect
import Html exposing (Html, div, text)
import Html.Attributes exposing (class, href, value)
import Http
import ImageList
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, hardcoded, required, requiredAt)
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase)
import Multiselect
import NavBar exposing (plantsLink, viewNav)
import Pages.Search exposing (Available, availableDecoder)
import Utils exposing (createdDecoder, existsDecoder, fillParent, flex, flex1, largeCentered, smallMargin)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = Add AddView
    | Edit EditView
    | BadEdit


type alias AddView =
    { available : WebData Available, plant : PlantView, result : Maybe (WebData Int) }


type alias EditView =
    { available : WebData Available, plant : WebData PlantView, plantId : Int, result : Maybe (WebData Bool) }


type alias PlantView =
    { name : String
    , description : String
    , created : String
    , regions : Multiselect.Model
    , soil : Int
    , group : Int
    , images : ImageList.Model
    , uploadedFiles : List File
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
    | GotPlant (Result Http.Error (Maybe PlantView))
    | Submit
    | GotSubmitAdd (Result Http.Error Int)


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

                setPlant plant =
                    case model of
                        Edit editView ->
                            case editView.plant of
                                Loaded loadedP ->
                                    authed <| Edit <| { editView | plant = Loaded plant }

                                _ ->
                                    m

                        Add addView ->
                            authed <| Add <| { addView | plant = plant }

                        BadEdit ->
                            m
            in
            case ( msg, model ) of
                ( StartUpload, _ ) ->
                    ( m, requestImages )

                ( GotAvailable (Ok res), Edit editView ) ->
                    ( authed <| Edit <| { editView | available = Loaded res }, getPlantCommand res auth.token editView.plantId )

                ( GotAvailable (Err err), Edit editView ) ->
                    ( authed <| Edit <| { editView | available = Error }, Cmd.none )

                ( GotAvailable (Ok res), Add addView ) ->
                    let
                        updatePlant plant =
                            { plant | regions = res.regions }
                    in
                    ( authed <| Add <| { addView | available = Loaded res, plant = updatePlant addView.plant }, Cmd.none )

                ( GotAvailable (Err err), Add addView ) ->
                    ( authed <| Add <| { addView | available = Error }, Cmd.none )

                ( GotPlant (Ok res), Edit editView ) ->
                    case res of
                        Just plView ->
                            ( authed <| Edit <| { editView | plant = Loaded plView }, Cmd.none )

                        Nothing ->
                            ( authed <| BadEdit, Cmd.none )

                ( GotPlant (Err err), Edit _ ) ->
                    ( authed <| BadEdit, Cmd.none )

                ( RegionsMS msEvent, Add addView ) ->
                    let
                        ( subModel, subCmd, _ ) =
                            Multiselect.update msEvent addView.plant.regions

                        updatedRegion plant =
                            { plant | regions = subModel }
                    in
                    ( authed <| Add <| { addView | plant = updatedRegion addView.plant }, Cmd.map RegionsMS subCmd )

                ( RegionsMS msEvent, Edit editView ) ->
                    case editView.plant of
                        Loaded plantView ->
                            let
                                ( subModel, subCmd, _ ) =
                                    Multiselect.update msEvent plantView.regions
                            in
                            ( authed <| Edit <| { editView | plant = Loaded { plantView | regions = subModel } }, Cmd.map RegionsMS subCmd )

                        _ ->
                            noOp

                ( ImagesLoaded file files, Edit editView ) ->
                    case editView.plant of
                        Loaded plantView ->
                            ( authed <| Edit <| { editView | plant = Loaded { plantView | uploadedFiles = files ++ [ file ] } }, Cmd.none )

                        _ ->
                            noOp

                ( ImagesLoaded file files, Add addView ) ->
                    let
                        updatedFiles plant =
                            { plant | uploadedFiles = files ++ [ file ] }
                    in
                    ( authed <| Add <| { addView | plant = updatedFiles addView.plant }, Cmd.none )

                ( Images imgEvent, Edit editView ) ->
                    case editView.plant of
                        Loaded plantView ->
                            ( authed <| Edit <| { editView | plant = Loaded { plantView | images = ImageList.update imgEvent plantView.images } }, Cmd.none )

                        _ ->
                            noOp

                ( NameUpdate newName, Add addView ) ->
                    let
                        updatedName plant =
                            { plant | name = newName }
                    in
                    ( authed <| Add <| { addView | plant = updatedName addView.plant }, Cmd.none )

                ( NameUpdate newName, Edit editView ) ->
                    let
                        updatedName plant =
                            { plant | name = newName }
                    in
                    case editView.plant of
                        Loaded pl ->
                            ( authed <| Edit <| { editView | plant = Loaded (updatedName pl) }, Cmd.none )

                        _ ->
                            noOp

                ( DescriptionUpdate newDesc, Add addView ) ->
                    let
                        updatedName plant =
                            { plant | description = newDesc }
                    in
                    ( authed <| Add <| { addView | plant = updatedName addView.plant }, Cmd.none )

                ( DescriptionUpdate newDesc, Edit editView ) ->
                    let
                        updatedName plant =
                            { plant | description = newDesc }
                    in
                    case editView.plant of
                        Loaded pl ->
                            ( authed <| Edit <| { editView | plant = Loaded (updatedName pl) }, Cmd.none )

                        _ ->
                            noOp

                ( SoilUpdate soil, _ ) ->
                    case model of
                        Add addView ->
                            let
                                updatePlant plant =
                                    { plant | soil = soil }
                            in
                            ( authed <| Add <| { addView | plant = updatePlant addView.plant }, Cmd.none )

                        Edit editView ->
                            case editView.plant of
                                Loaded plant ->
                                    let
                                        updatedPlant =
                                            { plant | soil = soil }
                                    in
                                    ( authed <| Edit <| { editView | plant = Loaded updatedPlant }, Cmd.none )

                                _ ->
                                    noOp

                        _ ->
                            noOp

                ( GroupUpdate group, Add { plant } ) ->
                    ( setPlant { plant | group = group }, Cmd.none )

                ( GroupUpdate group, Edit { plant } ) ->
                    case plant of
                        Loaded pl ->
                            ( setPlant { pl | group = group }, Cmd.none )

                        _ ->
                            noOp

                ( DateUpdate date, Add { plant } ) ->
                    ( setPlant { plant | created = date }, Cmd.none )

                ( Submit, Add addView ) ->
                    ( m, submitAddCommand auth.token addView.plant )

                ( GotSubmitAdd (Ok res), Add addView ) ->
                    ( authed <| Add <| { addView | result = Just (Loaded res) }, Cmd.none )

                ( GotSubmitAdd (Err err), Add addView ) ->
                    ( authed <| Add <| { addView | result = Just Error }, Cmd.none )

                ( _, _ ) ->
                    noOp

        _ ->
            noOp



--view


view : Model -> Html Msg
view model =
    viewNav model (Just plantsLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage resp page =
    case page of
        BadEdit ->
            div [] [ text "There is no such plant" ]

        Edit editView ->
            viewWebdata editView.plant (viewPlant editView.available True (div [ flex1 ] []))

        Add addView ->
            viewPlant addView.available False (viewResultAdd addView.result) addView.plant


viewResultAdd : Maybe (WebData Int) -> Html msg
viewResultAdd result =
    case result of
        Just web ->
            viewWebdata web
                (\data ->
                    div [ flex1, flex, Flex.col, class "text-success", Flex.alignItemsCenter, Flex.justifyEnd ]
                        [ div [] [ text ("Successfully created plant " ++ String.fromInt data) ]
                        , div []
                            [ Button.linkButton [ Button.primary, Button.attrs [ href ("/notPosted/" ++ String.fromInt data ++ "/edit") ] ] [ text "Go to edit" ]
                            ]
                        ]
                )

        Nothing ->
            div [ flex1 ] []


viewPlant : WebData Available -> Bool -> Html Msg -> PlantView -> Html Msg
viewPlant av isEdit resultView plant =
    viewWebdata av (viewPlantBase isEdit plant resultView)


viewPlantBase : Bool -> PlantView -> Html Msg -> Available -> Html Msg
viewPlantBase isEdit plant resultView av =
    div ([ flex, Flex.row ] ++ fillParent)
        [ div [ Flex.col, flex1, flex ] (leftView isEdit plant av)
        , div [ flex, Flex.col, flex1, Flex.justifyBetween, Flex.alignItemsCenter ] (rightView resultView isEdit plant)
        ]


rightView : Html Msg -> Bool -> PlantView -> List (Html Msg)
rightView resultView isEdit plant =
    let
        btnMsg =
            if isEdit then
                "Edit"

            else
                "Add"

        btnView =
            div [ flex1 ]
                [ Button.button
                    [ Button.primary
                    , Button.onClick Submit
                    , Button.attrs ([ smallMargin ] ++ largeCentered)
                    ]
                    [ text btnMsg ]
                ]

        imgView =
            div [ flex1 ]
                [ Html.map Images (ImageList.view plant.images)
                ]
    in
    if isEdit then
        [ imgView
        , resultView
        , btnView
        ]

    else
        [ resultView, btnView ]


leftView : Bool -> PlantView -> Available -> List (Html Msg)
leftView isEdit plant av =
    let
        filesText =
            case plant.uploadedFiles of
                [] ->
                    "No files selected"

                _ ->
                    String.join ", " (List.map File.name plant.uploadedFiles)

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

        dateInput =
            if isEdit then
                Input.text [ Input.disabled True, Input.value plant.created ]

            else
                Input.date [ Input.onInput DateUpdate, Input.value plant.created, Input.disabled isEdit ]
    in
    viewInput "Name" (Input.text [ Input.onInput NameUpdate, Input.value plant.name ])
        ++ viewInput "Add Image"
            (div [ flex, Flex.row ]
                [ Button.button
                    [ Button.primary, Button.onClick StartUpload, Button.attrs [ flex1 ] ]
                    [ text "Upload" ]
                , div [ flex1, smallMargin ] [ text filesText ]
                ]
            )
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
        ++ viewInput "Created Date" dateInput


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

                Add _ ->
                    getAvailable res.token

                Edit _ ->
                    getAvailable res.token
    in
    initBase [ Producer, Consumer, Manager ] initial initialCmd resp


decodeInitial flags =
    let
        isEdit =
            Result.withDefault False <| D.decodeValue (D.field "isEdit" D.bool) flags

        empyImageList =
            ImageList.fromDict (Dict.fromList [])

        emptyMultiSelect =
            Multiselect.initModel [] "regions" Multiselect.Show
    in
    if isEdit then
        case D.decodeValue (D.field "plantId" D.int) flags of
            Ok id ->
                Edit <| EditView Loading Loading id Nothing

            Err _ ->
                BadEdit

    else
        Add <| AddView Loading (PlantView "" "" "" emptyMultiSelect 1 1 empyImageList []) Nothing



--subs


subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none



--commands


requestImages : Cmd Msg
requestImages =
    FileSelect.files [ "image/png", "image/jpg" ] ImagesLoaded


getAvailable : String -> Cmd Msg
getAvailable token =
    Endpoints.getAuthed token Dicts (Http.expectJson GotAvailable availableDecoder) Nothing


getPlantCommand : Available -> String -> Int -> Cmd Msg
getPlantCommand av token plantId =
    let
        expect =
            Http.expectJson GotPlant (plantDecoder av token)
    in
    getAuthed token (NotPostedPlant plantId) expect Nothing


plantDecoder : Available -> String -> D.Decoder (Maybe PlantView)
plantDecoder av token =
    existsDecoder (plantDecoderBase av token)


plantDecoderBase : Available -> String -> D.Decoder PlantView
plantDecoderBase av token =
    let
        itemRequired name =
            requiredAt [ "item", name ]

        regions =
            Multiselect.getValues av.regions

        getPairWithKey key list =
            List.head (List.filter (\( k, v ) -> k == key) list)

        getRegionFor id =
            case getPairWithKey id regions of
                Just val ->
                    val

                Nothing ->
                    ( "-1", "Unknown" )

        regFunc id =
            getRegionFor (String.fromInt id)

        regIdsDecoder =
            D.at [ "item", "regions" ] (D.list D.int)

        selected ids =
            List.map regFunc ids

        reg ids =
            Multiselect.populateValues (Multiselect.initModel regions "regions" Multiselect.Show) regions (selected ids)

        regDecoder =
            D.map reg regIdsDecoder
    in
    D.succeed PlantView
        |> itemRequired "plantName" D.string
        |> itemRequired "description" D.string
        |> custom createdDecoder
        |> custom regDecoder
        |> itemRequired "soilId" D.int
        |> itemRequired "groupId" D.int
        |> custom (imagesDecoder token)
        |> hardcoded []


submitAddCommand : String -> PlantView -> Cmd Msg
submitAddCommand token plant =
    let
        expect =
            Http.expectJson GotSubmitAdd (D.field "id" D.int)
    in
    postAuthed token AddPlant (getAddBody plant) expect Nothing


getAddBody : PlantView -> Http.Body
getAddBody plant =
    Http.multipartBody
        ([ Http.stringPart "Name" plant.name
         , Http.stringPart "Description" plant.description
         , Http.stringPart "SoilId" (String.fromInt plant.soil)
         , Http.stringPart "GroupId" (String.fromInt plant.group)
         , Http.stringPart "Created" plant.created
         ]
            ++ regionsParts plant.regions
            ++ filesParts plant.uploadedFiles
        )


regionsParts : Multiselect.Model -> List Http.Part
regionsParts regions =
    let
        keys =
            List.map Tuple.first (Multiselect.getSelectedValues regions)
    in
    List.map (\key -> Http.stringPart "Regions" key) keys


filesParts : List File -> List Http.Part
filesParts files =
    List.map (Http.filePart "files") files



--main


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
