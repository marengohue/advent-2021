_ = require 'lodash'
input = require '../input.json'

min = _.min(input)
max = _.max(input)

fuelCount = (steps) -> 
    absSteps = Math.abs steps
    (1 + absSteps) / 2 * absSteps

res = _
    .chain _.range(min, max)
    .map (pos) -> 
        [
            pos,
            _
                .chain input
                .map (x) -> fuelCount(x - pos)
                .sum()
                .value()
        ]
    .minBy ([pos, fuelTotal]) -> fuelTotal
    .value()

console.log res
