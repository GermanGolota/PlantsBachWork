module Pages.Plant exposing (..)

import Bootstrap.Button as Button
import Bootstrap.Utilities.Flex as Flex
import Dict
import Endpoints exposing (Endpoint(..), getAuthed, imageIdToUrl)
import Html exposing (Html, div, text)
import Html.Attributes exposing (href, style)
import Http
import ImageList as ImageList
import Json.Decode as D
import Json.Decode.Pipeline exposing (custom, required, requiredAt)
import Main exposing (AuthResponse, ModelBase(..), UserRole(..), baseApplication, initBase, viewBase)
import NavBar exposing (searchLink, viewNav)
import Utils exposing (fillParent, flex, flex1, formatPrice, largeFont, mediumFont, mediumMargin, smallMargin, textCenter)
import Webdata exposing (WebData(..), viewWebdata)



--model


type alias Model =
    ModelBase View


type View
    = NoPlant
    | Plant PlantView


type alias PlantView =
    { id : Int
    , plantType : ViewType
    }


type ViewType
    = JustPlant (WebData (Maybe PlantModel))
    | Order (WebData (Maybe PlantModel)) (WebData (Maybe DeliveryAddress))


type alias DeliveryAddress =
    { city : String
    , location : String
    }



--update


type alias PlantModel =
    { name : String
    , description : String
    , price : Float
    , soil : String
    , regions : List String
    , group : String

    --{createdDate}({createdHumanDate})
    , created : String
    , sellerName : String
    , sellerPhone : String
    , sellerCreds : PersonCreds
    , caretakerCreds : PersonCreds
    , images : ImageList.Model
    }


type alias PersonCreds =
    { sold : Int
    , cared : Int
    , instructions : Int
    }


type Msg
    = GotPlant (Result Http.Error (Maybe PlantModel))
    | Images ImageList.Msg


update : Msg -> Model -> ( Model, Cmd Msg )
update msg m =
    case m of
        Authorized auth model ->
            let
                authed newModel =
                    Authorized auth newModel

                authedPlant plantView =
                    authed <| Plant plantView
            in
            case ( msg, model ) of
                ( GotPlant (Ok res), Plant p ) ->
                    case p.plantType of
                        JustPlant pWeb ->
                            ( authedPlant <| { p | plantType = JustPlant <| Loaded res }, Cmd.none )

                        Order pWeb del ->
                            ( authedPlant <| { p | plantType = Order (Loaded res) del }, Cmd.none )

                ( GotPlant (Err res), Plant p ) ->
                    case p.plantType of
                        JustPlant pWeb ->
                            ( authedPlant <| { p | plantType = JustPlant <| Error }, Cmd.none )

                        Order pWeb del ->
                            ( authedPlant <| { p | plantType = Order Error del }, Cmd.none )

                ( Images img, Plant p ) ->
                    case p.plantType of
                        JustPlant (Loaded (Just pl)) ->
                            ( authedPlant { p | plantType = JustPlant <| Loaded <| Just { pl | images = ImageList.update img pl.images } }, Cmd.none )

                        Order (Loaded (Just pl)) del ->
                            ( authedPlant { p | plantType = Order (Loaded <| Just { pl | images = ImageList.update img pl.images }) del }, Cmd.none )

                        _ ->
                            ( m, Cmd.none )

                ( _, _ ) ->
                    ( m, Cmd.none )

        _ ->
            ( m, Cmd.none )



--commands


getPlantCommand : String -> Int -> Cmd Msg
getPlantCommand token plantId =
    let
        expect =
            Http.expectJson GotPlant (plantDecoder token)
    in
    getAuthed token (PlantE plantId) expect Nothing


plantDecoder : String -> D.Decoder (Maybe PlantModel)
plantDecoder token =
    let
        initedDecoder =
            plantItemDecoder token
    in
    D.field "exists" D.bool |> D.andThen initedDecoder


plantItemDecoder : String -> Bool -> D.Decoder (Maybe PlantModel)
plantItemDecoder token exists =
    if exists then
        D.map Just (plantDecoderBase token)

    else
        D.succeed Nothing


plantDecoderBase : String -> D.Decoder PlantModel
plantDecoderBase token =
    let
        requiredItem name =
            requiredAt [ "item", name ]
    in
    D.succeed PlantModel
        |> requiredItem "plantName" D.string
        |> requiredItem "description" D.string
        |> requiredItem "price" D.float
        |> requiredItem "soilName" D.string
        |> requiredItem "regions" (D.list D.string)
        |> requiredItem "groupName" D.string
        |> custom createdDecoder
        |> requiredItem "sellerName" D.string
        |> requiredItem "sellerPhone" D.string
        |> custom (credsDecoder "seller")
        |> custom (credsDecoder "careTaker")
        |> custom (imagesDecoder token)


imagesDecoder : String -> D.Decoder ImageList.Model
imagesDecoder token =
    let
        baseDecoder =
            imageIdsToModel token
    in
    D.map baseDecoder (D.at [ "item", "images" ] (D.list D.int))


imageIdsToModel : String -> List Int -> ImageList.Model
imageIdsToModel token ids =
    let
        baseList =
            List.map (\id -> ( id, imageIdToUrl token id )) ids
    in
    ImageList.fromDict <| Dict.fromList baseList


credsDecoder : String -> D.Decoder PersonCreds
credsDecoder person =
    let
        combine str =
            person ++ str

        requiredItem val =
            requiredAt [ "item", val ]
    in
    D.succeed PersonCreds
        |> requiredItem (combine "Cared") D.int
        |> requiredItem (combine "Sold") D.int
        |> requiredItem (combine "Instructions") D.int


createdDecoder : D.Decoder String
createdDecoder =
    let
        combine date humanDate =
            date ++ "(" ++ humanDate ++ ")"
    in
    D.map2
        combine
        (D.at [ "item", "createdDate" ] D.string)
        (D.at [ "item", "createdHumanDate" ] D.string)



--view


view : Model -> Html Msg
view model =
    viewNav model (Just searchLink) viewPage


viewPage : AuthResponse -> View -> Html Msg
viewPage _ page =
    case page of
        NoPlant ->
            div [] [ text "Please select a plant", Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href "/search" ] ] [ text "Return to search" ] ]

        Plant plant ->
            let
                plantView =
                    viewPlantFull plant.id
            in
            case plant.plantType of
                JustPlant pl ->
                    viewWebdata pl plantView

                Order pl _ ->
                    viewWebdata pl plantView


viewPlantFull : Int -> Maybe PlantModel -> Html Msg
viewPlantFull id p =
    case p of
        Just plant ->
            viewPlant id plant

        Nothing ->
            div [] [ text "This plant is no longer available, sorry" ]


viewPlant : Int -> PlantModel -> Html Msg
viewPlant id plant =
    let
        filled args =
            fillParent ++ args

        largeCentered =
            [ largeFont, textCenter ]
    in
    div (filled [ flex, Flex.row ])
        [ div [ flex, Flex.col, flex1 ]
            [ div largeCentered [ text plant.name ]
            , Html.map Images <| ImageList.view plant.images
            , viewPlantStat "Caretaker Credentials" (credsToString plant.caretakerCreds)
            , viewPlantStat "Seller Credentials" (credsToString plant.sellerCreds)
            , viewPlantStat "Seller Nama" plant.sellerName
            , viewPlantStat "Seller Phone" plant.sellerPhone
            ]
        , div [ flex, Flex.col, flex1 ]
            [ div largeCentered [ text "Description" ]
            , div [] [ text plant.description ]
            , div largeCentered [ text <| formatPrice plant.price ]
            , viewPlantStat "Soil" plant.soil
            , viewPlantStat "Regions" (String.join ", " plant.regions)
            , viewPlantStat "Group" plant.group
            , viewPlantStat "Age" plant.created
            , interactionButtons id
            ]
        ]


credsToString : PersonCreds -> String
credsToString creds =
    String.fromInt creds.cared ++ " cared, " ++ String.fromInt creds.sold ++ " sold, " ++ String.fromInt creds.instructions ++ " instructions published"


viewPlantStat desc value =
    flexRowGap (div [ mediumMargin, mediumFont ] [ text desc ]) (div [ mediumMargin, mediumFont ] [ text value ])


flexRowGap left right =
    div [ flex, Flex.row, Flex.justifyBetween, flex1, Flex.alignItemsCenter ]
        [ left
        , right
        ]


interactionButtons id =
    div [ flex, style "margin" "3em", Flex.row, Flex.justifyEnd ]
        [ Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href <| "/search", largeFont ] ] [ text "Back" ]
        , Button.linkButton [ Button.primary, Button.attrs [ smallMargin, href <| "/order/" ++ String.fromInt id, largeFont ] ] [ text "Order" ]
        ]



--init


init : Maybe AuthResponse -> D.Value -> ( Model, Cmd Msg )
init resp flags =
    let
        initialModel =
            decodeInitial flags

        cmds authResp =
            case initialModel of
                Plant p ->
                    getPlantCommand authResp.token p.id

                NoPlant ->
                    Cmd.none
    in
    initBase
        [ Producer, Consumer, Manager ]
        initialModel
        cmds
        resp


decodeInitial : D.Value -> View
decodeInitial flags =
    case decodePlantId flags of
        Err _ ->
            NoPlant

        Ok plantId ->
            case String.toInt plantId of
                Just plantNumber ->
                    Plant (PlantView plantNumber <| JustPlant Loading)

                Nothing ->
                    NoPlant


decodePlantId : D.Value -> Result D.Error String
decodePlantId flags =
    D.decodeValue (D.field "plantId" D.string) flags


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
