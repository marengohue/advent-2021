module Types

type public SnailNum =
| Single of int
| Pair of SnailNum*SnailNum

type public CarryOver =
| Carry of int
| None
| Spread
