module Pages.Search exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Card as Card
import Bootstrap.Card.Block as Block
import Bootstrap.Form.Input as Input
import Bootstrap.Utilities.Flex as Flex
import Dict exposing (Dict)
import Endpoints exposing (Endpoint(..), endpointToUrl, getAuthedQuery, imageIdToUrl)
import Html exposing (Html, div, i, text)
import Html.Attributes exposing (alt, class, href, src, style)
import Http
import Json.Decode as D
import Json.Decode.Pipeline exposing (required)
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase, viewBase)
import Multiselect exposing (InputInMenu(..))
import NavBar exposing (viewNav)
import Utils exposing (fillParent, flex, formatPrice, largeFont, smallMargin, textCenter)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type alias View =
    { searchItems : Dict String String
    , availableValues : WebData Available
    , results : Maybe (WebData (List SearchResultItem))
    }


type alias Available =
    { regions : Multiselect.Model
    , soils : Multiselect.Model
    , groups : Multiselect.Model
    }


buildQuery : List ( String, String ) -> String
buildQuery items =
    List.foldl addQuery "?" items


addQuery : ( String, String ) -> String -> String
addQuery ( key, value ) result =
    result ++ key ++ "=" ++ value ++ "&"


type alias SearchResultItem =
    { id : Int
    , name : String
    , description : String
    , price : Float
    , imageIds : List Int
    }



--update


type Msg
    = SetQuery String String
    | GotSearch (Result Http.Error (List SearchResultItem))
    | GotAvailable (Result Http.Error Available)
    | RegionsMS Multiselect.Msg
    | SoilMS Multiselect.Msg
    | GroupMS Multiselect.Msg


update : Msg -> Model -> ( Model, Cmd Msg )
update msg m =
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
                            (setQuery key value model) 

                        updatedView =
                            {queried | results = Just Loading}
                    in
                    ( Authorized auth updatedView, searchFull updatedView.searchItems updatedView.availableValues auth.token )

                ( GotSearch (Ok res), _ ) ->
                    ( updateData model auth <| Loaded res, Cmd.none )

                ( GotSearch (Err err), _ ) ->
                    ( updateData model auth Error, Cmd.none )

                ( GotAvailable (Ok res), _ ) ->
                    ( authed { model | availableValues = Loaded res }, Cmd.none )

                ( GotAvailable (Err err), _ ) ->
                    ( authed { model | availableValues = Error }, Cmd.none )

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
                                {setNew | results = Just Loading}

                            else
                                setNew


                        searchCmd =
                            if availableChanged then
                                searchFull updatedView.searchItems updatedView.availableValues auth.token

                            else
                                Cmd.none
                    in
                    ( authed updatedView, Cmd.batch [ Cmd.map RegionsMS subCmd, searchCmd ] )

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
                                {setNew | results = Just Loading}
                            else
                                setNew

                        searchCmd =
                            if availableChanged then
                                searchFull updatedView.searchItems updatedView.availableValues auth.token

                            else
                                Cmd.none
                    in
                    ( authed updatedView, Cmd.batch [ Cmd.map SoilMS subCmd, searchCmd ] )

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
                                {setNew | results = Just Loading}
                            else
                                setNew

                        searchCmd =
                            if availableChanged then
                                searchFull updatedView.searchItems updatedView.availableValues auth.token

                            else
                                Cmd.none
                    in
                    ( authed updatedView, Cmd.batch [ Cmd.map GroupMS subCmd, searchCmd ] )

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
            map "RegionIds" (Multiselect.getSelectedValues av.regions)

        soils =
            map "SoilIds" <| Multiselect.getSelectedValues av.soils

        groups =
            map "GroupIds" <| Multiselect.getSelectedValues av.groups
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
    Authorized auth <| View model.searchItems model.availableValues <| Just data


setQuery : String -> String -> View -> View
setQuery key value viewType =
    View (Dict.update key (\_ -> Just value) viewType.searchItems) viewType.availableValues viewType.results



--commands


getAvailable : String -> Cmd Msg
getAvailable token =
    Endpoints.getAuthed token Dicts (Http.expectJson GotAvailable availableDecoder) Nothing


search : List ( String, String ) -> String -> Cmd Msg
search items token =
    Endpoints.getAuthedQuery (buildQuery items) token Search (Http.expectJson GotSearch searchResultsDecoder) Nothing


searchResultsDecoder : D.Decoder (List SearchResultItem)
searchResultsDecoder =
    D.field "items" <| D.list searchResultDecoder


searchResultDecoder : D.Decoder SearchResultItem
searchResultDecoder =
    D.succeed SearchResultItem
        |> required "id" D.int
        |> required "plantName" D.string
        |> required "description" D.string
        |> required "price" D.float
        |> required "imageIds" (D.list D.int)


availableDecoder : D.Decoder Available
availableDecoder =
    D.map3 Available
        ((D.field "regions" <| D.dict D.string)
            |> dictDecoder "region"
        )
        ((D.field "soils" <| D.dict D.string)
            |> dictDecoder "soil"
        )
        (D.field "groups" (D.dict D.string)
            |> dictDecoder "group"
        )


dictDecoder : String -> D.Decoder (Dict String String) -> D.Decoder Multiselect.Model
dictDecoder tag base =
    D.map (convertDict tag) base


convertDict : String -> Dict String String -> Multiselect.Model
convertDict tag dict =
    Multiselect.initModel (Dict.toList dict) tag Show



--view


view : Model -> Html Msg
view model =
    viewNav model (Just NavBar.searchLink) pageView


pageView : AuthResponse -> View -> Html Msg
pageView resp viewType =
    let
        viewFunc =
            resultsView resp.token

        result =
            case viewType.results of
                Just res ->
                    viewWebdata res viewFunc

                Nothing ->
                    div [] [ text "No search is selected" ]
    in
    div ([ flex, Flex.col ] ++ fillParent)
        [ viewWebdata viewType.availableValues viewAvailable
        , div [ Flex.row, flex ]
            [ viewInput "Plant Name" <| Input.text [ Input.onInput (\val -> SetQuery "PlantName" val) ]
            , viewInput "Price" <|
                div [ Flex.row, flex, Flex.alignItemsCenter ]
                    [ Input.text [ Input.onInput (\val -> SetQuery "LowerPrice" val), Input.attrs [style "text-align" "right"] ]
                    , i [ class "fa-solid fa-minus", smallMargin ] []
                    , Input.text [ Input.onInput (\val -> SetQuery "TopPrice" val), Input.attrs [style "text-align" "right"] ]
                    ]
            , viewInput "Created Before" <| Input.date [ Input.onInput (\val -> SetQuery "LastDate" val) ]
            ]
        , result
        ]


viewAvailable : Available -> Html Msg
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


resultsView : String -> List SearchResultItem -> Html Msg
resultsView token items =
    let
        viewFunc =
            resultView token
    in
    Utils.chunkedView 3 viewFunc items


resultView : String -> SearchResultItem -> Html Msg
resultView token item =
    let
        url =
            imageIdToUrl token (Maybe.withDefault -1 (List.head item.imageIds))
    in
    Card.config [ Card.attrs (fillParent ++ [ style "flex" "1" ]) ]
        |> Card.header [ class "text-center" ]
            [ Html.img ([ src url, alt "No images for this plant" ] ++ fillParent) []
            ]
        |> Card.block []
            [ Block.titleH4 [] [ text item.name ]
            , Block.text [] [ text item.description ]
            , Block.custom <|
                div [ flex, Flex.row, style "justify-content" "space-between", Flex.alignItemsCenter ]
                    [ div [ largeFont ] [ text <| formatPrice item.price ]
                    , div [ flex, Flex.row ]
                        [ Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href <| "/plant/" ++ String.fromInt item.id ++ "/order" ] ] [ text "Order" ]
                        , Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href <| "/plant/" ++ String.fromInt item.id ] ] [ text "Open" ]
                        ]
                    ]
            ]
        |> Card.view


viewInput : String -> Html Msg -> Html Msg
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
    initBase [ Producer, Consumer, Manager ] (View (Dict.fromList []) Loading Nothing) cmds resp


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
