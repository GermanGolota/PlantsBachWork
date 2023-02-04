module Pages.Search exposing (..)

import Available exposing (Available, availableDecoder)
import Bootstrap.Button as Button
import Bootstrap.Card as Card
import Bootstrap.Card.Block as Block
import Bootstrap.Form.Input as Input
import Bootstrap.Utilities.Flex as Flex
import Dict exposing (Dict)
import Endpoints exposing (Endpoint(..), historyUrl, imagesDecoder, postAuthed)
import Html exposing (Html, div, i, text)
import Html.Attributes exposing (alt, class, src, style)
import Http
import ImageList
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, hardcoded, required)
import Main exposing (AuthResponse, ModelBase(..), MsgBase(..), UserRole(..), baseApplication, initBase, isAdmin, mapCmd, updateBase)
import Multiselect exposing (InputInMenu(..))
import NavBar exposing (viewNav)
import Utils exposing (buildQuery, decodeId, fillParent, flex, flex1, formatPrice, intersect, largeCentered, mediumFont, smallMargin, textCenter)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type alias View =
    { searchItems : Dict String String
    , availableValues : WebData Available
    , results : WebData (List SearchResultItem)
    }


type alias SearchResultItem =
    { id : String
    , name : String
    , description : String
    , price : Float
    , images : ImageList.Model
    , wasAbleToDelete : Maybe (WebData Bool)
    }



--update


type LocalMsg
    = SetQuery String String
    | GotSearch (Result Http.Error (List SearchResultItem))
    | GotAvailable (Result Http.Error Available)
    | RegionsMS Multiselect.Msg
    | SoilMS Multiselect.Msg
    | GroupMS Multiselect.Msg
    | SelectedDeletePost String
    | GotDeletePost String (Result Http.Error Bool)
    | ImageSelected String ImageList.Msg


type alias Msg =
    MsgBase LocalMsg


update =
    updateBase updateLocal


updateLocal : LocalMsg -> Model -> ( Model, Cmd Msg )
updateLocal msg m =
    case m of
        Authorized auth model ->
            let
                authed viewType =
                    Authorized auth viewType

                availableList available =
                    case available of
                        Loaded av ->
                            availableToList av

                        _ ->
                            []

                searchFull items available token =
                    search (availableList available ++ Dict.toList items) token
            in
            case ( msg, model.availableValues ) of
                ( SetQuery key value, _ ) ->
                    let
                        queried =
                            setQuery key value model

                        updatedView =
                            { queried | results = Loading }
                    in
                    ( Authorized auth updatedView, searchFull updatedView.searchItems updatedView.availableValues auth.token )

                ( GotSearch (Ok res), _ ) ->
                    ( updateData model auth <| Loaded res, Cmd.none )

                ( GotSearch (Err err), _ ) ->
                    ( updateData model auth <| Error err, Cmd.none )

                ( GotAvailable (Ok res), _ ) ->
                    ( authed { model | availableValues = Loaded res }, Cmd.none )

                ( GotAvailable (Err err), _ ) ->
                    ( authed { model | availableValues = Error err }, Cmd.none )

                ( RegionsMS sub, Loaded val ) ->
                    let
                        ( subModel, subCmd, _ ) =
                            Multiselect.update sub val.regions

                        newVal =
                            updateAvailableRegion val subModel

                        setNew =
                            { model | availableValues = Loaded newVal }

                        availableChanged =
                            Multiselect.getSelectedValues val.regions /= Multiselect.getSelectedValues newVal.regions

                        updatedView =
                            if availableChanged then
                                { setNew | results = Loading }

                            else
                                setNew

                        searchCmd =
                            if availableChanged then
                                searchFull updatedView.searchItems updatedView.availableValues auth.token

                            else
                                Cmd.none
                    in
                    ( authed updatedView, Cmd.batch [ Cmd.map RegionsMS subCmd |> mapCmd, searchCmd ] )

                ( SoilMS sub, Loaded val ) ->
                    let
                        ( subModel, subCmd, _ ) =
                            Multiselect.update sub val.soils

                        newVal =
                            updateAvailableSoil val subModel

                        availableChanged =
                            Multiselect.getSelectedValues val.soils /= Multiselect.getSelectedValues newVal.soils

                        setNew =
                            { model | availableValues = Loaded newVal }

                        updatedView =
                            if availableChanged then
                                { setNew | results = Loading }

                            else
                                setNew

                        searchCmd =
                            if availableChanged then
                                searchFull updatedView.searchItems updatedView.availableValues auth.token

                            else
                                Cmd.none
                    in
                    ( authed updatedView, Cmd.batch [ Cmd.map SoilMS subCmd |> mapCmd, searchCmd ] )

                ( GroupMS sub, Loaded val ) ->
                    let
                        ( subModel, subCmd, _ ) =
                            Multiselect.update sub val.groups

                        newVal =
                            updateAvailableGroup val subModel

                        availableChanged =
                            Multiselect.getSelectedValues val.groups /= Multiselect.getSelectedValues newVal.groups

                        setNew =
                            { model | availableValues = Loaded newVal }

                        updatedView =
                            if availableChanged then
                                { setNew | results = Loading }

                            else
                                setNew

                        searchCmd =
                            if availableChanged then
                                searchFull updatedView.searchItems updatedView.availableValues auth.token

                            else
                                Cmd.none
                    in
                    ( authed updatedView, Cmd.batch [ Cmd.map GroupMS subCmd |> mapCmd, searchCmd ] )

                ( SelectedDeletePost id, Loaded val ) ->
                    let
                        updateResult result =
                            if result.id == id then
                                { result | wasAbleToDelete = Just Loading }

                            else
                                result

                        updatedList =
                            case model.results of
                                Loaded results ->
                                    List.map updateResult results

                                _ ->
                                    []
                    in
                    ( authed { model | results = Loaded updatedList }, deletePlant auth.token id )

                ( GotDeletePost id (Err err), Loaded _ ) ->
                    case model.results of
                        Loaded vals ->
                            let
                                updateResult resultItem =
                                    if resultItem.id == id then
                                        { resultItem | wasAbleToDelete = Just <| Error err }

                                    else
                                        resultItem

                                updatedResults =
                                    Loaded <| List.map updateResult vals
                            in
                            ( authed <| View model.searchItems model.availableValues updatedResults, Cmd.none )

                        _ ->
                            ( m, Cmd.none )

                ( GotDeletePost id (Ok res), Loaded val ) ->
                    if res then
                        ( m, searchFull model.searchItems model.availableValues auth.token )

                    else
                        case model.results of
                            Loaded vals ->
                                let
                                    updateResult resultItem =
                                        if resultItem.id == id then
                                            { resultItem | wasAbleToDelete = Just (Loaded res) }

                                        else
                                            resultItem

                                    updatedResults =
                                        Loaded <| List.map updateResult vals
                                in
                                ( authed <| View model.searchItems model.availableValues updatedResults, Cmd.none )

                            _ ->
                                ( m, Cmd.none )

                ( ImageSelected plantId image, Loaded val ) ->
                    case model.results of
                        Loaded vals ->
                            let
                                updateResult resultItem =
                                    if resultItem.id == plantId then
                                        { resultItem | images = ImageList.update image resultItem.images }

                                    else
                                        resultItem

                                updatedResults =
                                    Loaded <| List.map updateResult vals
                            in
                            ( authed <| View model.searchItems model.availableValues updatedResults, Cmd.none )

                        _ ->
                            ( m, Cmd.none )

                ( _, _ ) ->
                    ( m, Cmd.none )

        _ ->
            ( m, Cmd.none )


availableToList : Available -> List ( String, String )
availableToList av =
    let
        map text pairs =
            List.map (\( key, _ ) -> ( text, key )) pairs

        regions =
            map "RegionNames" <| Multiselect.getSelectedValues av.regions

        soils =
            map "SoilNames" <| Multiselect.getSelectedValues av.soils

        groups =
            map "GroupNames" <| Multiselect.getSelectedValues av.groups
    in
    regions ++ soils ++ groups


updateAvailableRegion : Available -> Multiselect.Model -> Available
updateAvailableRegion av model =
    { av | regions = model }


updateAvailableSoil : Available -> Multiselect.Model -> Available
updateAvailableSoil av model =
    { av | soils = model }


updateAvailableGroup : Available -> Multiselect.Model -> Available
updateAvailableGroup av model =
    { av | groups = model }


updateData : View -> AuthResponse -> WebData (List SearchResultItem) -> Model
updateData model auth data =
    Authorized auth <| View model.searchItems model.availableValues <| data


setQuery : String -> String -> View -> View
setQuery key value viewType =
    View (Dict.update key (\_ -> Just value) viewType.searchItems) viewType.availableValues viewType.results



--commands


deletePlant : String -> String -> Cmd Msg
deletePlant token id =
    postAuthed token (DeletePost id) Http.emptyBody (Http.expectJson (GotDeletePost id) deletedDecoder) Nothing |> mapCmd


deletedDecoder =
    D.field "success" D.bool


getAvailable : String -> Cmd Msg
getAvailable token =
    Endpoints.getAuthed token Dicts (Http.expectJson GotAvailable availableDecoder) Nothing |> mapCmd


search : List ( String, String ) -> String -> Cmd Msg
search items token =
    Endpoints.getAuthedQuery (buildQuery items) token Search (Http.expectJson GotSearch <| searchResultsDecoder token) Nothing |> mapCmd


searchResultsDecoder : String -> D.Decoder (List SearchResultItem)
searchResultsDecoder token =
    D.field "items" <| D.list <| searchResultDecoder token


searchResultDecoder : String -> D.Decoder SearchResultItem
searchResultDecoder token =
    D.succeed SearchResultItem
        |> required "id" decodeId
        |> required "plantName" D.string
        |> required "description" D.string
        |> required "price" D.float
        |> custom (imagesDecoder token [ "images" ])
        |> hardcoded Nothing


convertIds : List Int -> List String
convertIds ids =
    List.map String.fromInt ids



--view


view model =
    viewNav model (Just NavBar.searchLink) pageView


pageView : AuthResponse -> View -> Html Msg
pageView resp viewType =
    let
        viewFunc =
            resultsView (isAdmin resp) (List.member Consumer resp.roles) (intersect [ Manager, Producer ] resp.roles) resp.token
    in
    div ([ flex, Flex.col ] ++ fillParent)
        [ viewWebdata viewType.availableValues viewAvailable |> Html.map Main
        , div [ Flex.row, flex ]
            [ viewInput "Plant Name" <| Input.text [ Input.onInput (\val -> SetQuery "PlantName" val) ]
            , viewInput "Price" <|
                div [ Flex.row, flex, Flex.alignItemsCenter ]
                    [ Input.text [ Input.onInput (\val -> SetQuery "LowerPrice" val), Input.attrs [ style "text-align" "right" ] ]
                    , i [ class "fa-solid fa-minus", smallMargin ] []
                    , Input.text [ Input.onInput (\val -> SetQuery "TopPrice" val), Input.attrs [ style "text-align" "right" ] ]
                    ]
            , viewInput "Created Before" <| Input.date [ Input.onInput (\val -> SetQuery "LastDate" val) ]
            ]
            |> Html.map Main
        , div [ style "overflow-y" "scroll" ] [ viewWebdata viewType.results viewFunc ]
        ]


viewAvailable : Available -> Html LocalMsg
viewAvailable av =
    let
        viewMultiselectInput text convert model =
            viewInput text (multiSelectInput convert model)
    in
    div [ flex, Flex.row, style "width" "100%" ]
        [ viewMultiselectInput "Groups" GroupMS av.groups
        , viewMultiselectInput "Soils" SoilMS av.soils
        , viewMultiselectInput "Regions" RegionsMS av.regions
        ]


resultsView : Bool -> Bool -> Bool -> String -> List SearchResultItem -> Html Msg
resultsView isAdmin showOrder showDelete token items =
    let
        viewFunc =
            resultView isAdmin showOrder showDelete token
    in
    Utils.chunkedView 3 viewFunc items


resultView : Bool -> Bool -> Bool -> String -> SearchResultItem -> Html Msg
resultView isAdmin showOrder showDelete token item =
    let
        orderBtn =
            div [ flex, Flex.col, flex1 ]
                [ Button.linkButton
                    [ Button.primary
                    , Button.onClick <| Navigate ("/plant/" ++ item.id ++ "/order")
                    , Button.disabled (not showOrder)
                    ]
                    [ text "Order" ]
                ]

        msgText val =
            if val then
                div largeCentered [ text "Successfully Deleted" ]

            else
                div largeCentered [ text "Failed to delete" ]

        msgItem =
            case item.wasAbleToDelete of
                Just val ->
                    viewWebdata val msgText

                Nothing ->
                    div [] []

        deleteBtn =
            if showDelete then
                div [ flex, Flex.col, flex1 ]
                    [ msgItem
                    , Button.button [ Button.onClick (SelectedDeletePost item.id), Button.danger ] [ text "Remove" ]
                    ]

            else
                div [] []

        historyBtn =
            if isAdmin then
                div [ flex, Flex.row, flex1 ]
                    [ Button.linkButton
                        [ Button.outlinePrimary
                        , Button.onClick <| Navigate <| historyUrl "PlantPost" item.id
                        , Button.attrs [ smallMargin ]
                        ]
                        [ text "View history" ]
                    ]

            else
                div [] []
    in
    Card.config [ Card.attrs (fillParent ++ [ style "flex" "1" ]) ]
        |> Card.header [ class "text-center" ]
            [ ImageList.view item.images |> Html.map (\msg -> Main <| ImageSelected item.id msg)
            ]
        |> Card.block []
            [ Block.titleH4 [] [ text item.name ]
            , Block.text [] [ text item.description ]
            , Block.custom <|
                div [ flex, Flex.row, Flex.alignItemsCenter, Flex.justifyCenter ]
                    [ div [ flex, Flex.col, flex1, mediumFont ] [ text <| formatPrice item.price ]
                    , div [ flex, Flex.col, flex1 ] [ orderBtn ]
                    , div [ flex, Flex.col, flex1 ]
                        [ Button.linkButton
                            [ Button.primary
                            , Button.onClick <| Navigate ("/plant/" ++ item.id)
                            ]
                            [ text "Open" ]
                        ]
                    , div [ flex, Flex.col, flex1 ] [ div [ flex, Flex.row ] [ deleteBtn |> Html.map Main ], historyBtn ]
                    ]
            ]
        |> Card.view


viewInput : String -> Html LocalMsg -> Html LocalMsg
viewInput title input =
    div [ Flex.col, style "flex" "1", smallMargin ] [ div [ textCenter ] [ text title ], input ]


multiSelectInput : (Multiselect.Msg -> msg) -> Multiselect.Model -> Html msg
multiSelectInput msg model =
    Html.map msg <| Multiselect.view model


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp _ =
    let
        cmds authResp =
            Cmd.batch [ getAvailable authResp.token, search [] authResp.token ]
    in
    initBase [ Producer, Consumer, Manager ] (View (Dict.fromList []) Loading Loading) cmds resp


subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none


main : Program D.Value Model Msg
main =
    baseApplication
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
