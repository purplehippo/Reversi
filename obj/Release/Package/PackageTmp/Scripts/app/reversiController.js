//var reversiGameController = function ($scope) {
//    $scope.tiles = [{ id: 1 }, { id: 2 }];
//};

//function reversiGameController($scope) {
//    $scope.test = 'here i am';
//    $scope.tile = [{ id: 1 }, { id: 2 }];
//}

var reversiGame = angular.module('reversiGame', []);

// add $scope etc to array, so if minified we do not lose dependency on $scope as parameter
reversiGame.controller('reversiGameController', ['$scope', function ($scope) {
        $scope.test = 'here i am';
        $scope.tiles = [{ id: 1 }, { id: 2 }];
    }]);