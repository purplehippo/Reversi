/**
Reference: http://nickberardi.com/signalr-and-angularjs/
**/
var Reversi;
var width = height = 8;
(function (Reversi) {
    var app = angular.module('reversiGame', []);

    var gameController = (function () {
        function gameController($scope) {
            var _this = this;       // hang on to 'current' this object
            this.$scope = $scope;   // give us access to $scope from other functions
            this._hub = $.connection.broadcastHub;
            var that = this;

            $scope.tiles = initialiseBoard();
            $scope.boardWidth = width * 54;
            $scope.message = "";

            $scope.tileSelected = function (tile) {
                that.tileSelected(tile);
            };
            this._hub.client.updateGameMessages = function (messages, state) {
                console.log("client.updateGameMessages" + messages.toString() + " : " + state);
                return _this.updateGameMessages(messages, state);
            };

            $.connection.hub.start().done(function () {
                console.log("hub started");
            }).fail(function () {
                console.log("hub failed to start");
            });
        }

        gameController.prototype.tileSelected = function (tile) {
            if (typeof tile === "undefined") { return; }
            var that = this;
            return this._hub.server.play(tile.id).done(function (data) {
                that.$scope.message = that.$scope.message + " play(tile) done" + data;
                //            that.$scope.$apply();
            });
        };
        gameController.prototype.updateGameMessages = function (messages, state) {
            $scope.message = $scope.message + messages;
            $scope.$apply();
        };

        gameController.$inject = ['$scope'];
        return gameController;

    })();


    Reversi.gameController = gameController;
    app.controller("gameController", gameController);

})(Reversi || (Reversi = {}));



//// add $scope etc to array, so if minified we do not lose dependency on $scope as parameter


function initialiseBoard() {
    var tiles = [], tile;
    var middle = width / 2;
    var initialWhite = (middle - 1).toString() + (middle).toString() +
        "," + (middle).toString() + (middle - 1).toString();
    var initialBlack = (middle - 1).toString() + (middle - 1).toString() +
        "," + (middle).toString() + (middle).toString();
    for (var i = 0; i < width; i++) {
        for (var j = 0; j < height; j++) {
            tile = { id: i.toString() + j.toString(), state: "EMPTY" };

            if (initialBlack.indexOf(tile.id) > -1)
                tile.state = "WHITE";
            if (initialWhite.indexOf(tile.id) > -1)
                tile.state = "BLACK";

            tiles.push(tile);
        }
    }

    //// set the centre ones to black/white
    //GetTile(Constants.ROWS / 2 - 1, Constants.COLS / 2 - 1).State = TileStateEnum.WHITE;
    //GetTile(Constants.ROWS / 2 - 1, Constants.COLS / 2).State = TileStateEnum.BLACK;
    //GetTile(Constants.ROWS / 2, Constants.COLS / 2 - 1).State = TileStateEnum.BLACK;
    //GetTile(Constants.ROWS / 2, Constants.COLS / 2).State = TileStateEnum.WHITE;

    return tiles;
}