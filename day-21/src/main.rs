#![feature(hash_drain_filter)]

use std::collections::{HashMap, HashSet};
use itertools::Itertools;
use uint::construct_uint;

use dirac::DiracDie;
use dirac_coin::DiracCoin;
use die::Die;

mod input;
mod die;
mod dirac;
mod dirac_coin;

const BOARD_SIZE: u8 = 10;

const TARGET_SCORE: u32 = 21;

construct_uint! {
    pub struct U256(4);
}

#[derive(PartialEq, Eq, Hash, Debug)]
struct TurnOutcome {
    score: u64,
    pos: u8
}

fn turn(outcomes: HashMap<TurnOutcome, U256>, die: &mut impl Die) -> HashMap<TurnOutcome, U256> {
    let mut outcomes_next_turn = HashMap::<TurnOutcome, U256>::new();
    outcomes
        .iter()
        .cartesian_product(die.roll_turn().iter())
        .for_each(|((this_turn_outcome, outcome_count), roll)| {
            let landed_at = (this_turn_outcome.pos + roll.toss_sum - 1) % BOARD_SIZE + 1;
            let next_turn_outcomes = TurnOutcome {
                pos: landed_at,
                score: this_turn_outcome.score + (landed_at as u64)
            };
            let next_turn_outcome_count = outcome_count * roll.outcome_count as u64;

            outcomes_next_turn
                .entry(next_turn_outcomes)
                .and_modify(|e| *e += next_turn_outcome_count)
                .or_insert(U256::from(next_turn_outcome_count));
        });
    
    outcomes_next_turn
}

fn new_game(starting_pos: u8) -> HashMap<TurnOutcome, U256> {
    let mut new_game = HashMap::<TurnOutcome, U256>::new();
    new_game.insert(TurnOutcome { pos: starting_pos, score: 0 }, U256::from(1));
    new_game
}

fn main() {
    let (p1, p2) = input::read_input();
    let mut die = DiracDie::new();

    let game_p1 = new_game(p1);
    let game_p2 = new_game(p2);

    let mut total_p1_win_count = U256::from(0_u8);
    let mut total_p2_win_count = U256::from(0_u8);
    
    (0..15_u8)
        .fold((game_p1, game_p2), |(prev_p1, prev_p2), _| {
            let mut p1_result = turn(prev_p1, &mut die);
            let mut p1_win_count = U256::from(0_u8);
            let mut p1_no_win_count = U256::from(0_u8);
            let mut p2_win_count = U256::from(0_u8);
            
            p1_result
                .drain_filter(|turn_outcome, &mut outcome_count| {
                    if turn_outcome.score >= TARGET_SCORE.into() {
                        p1_win_count += outcome_count;
                        true
                    } else {
                        p1_no_win_count += outcome_count;
                        false
                    }
                });

            let prev_p2_outcomes = prev_p2
                .iter()
                .map(|(_, prev_outcomes)| prev_outcomes)
                .fold(U256::from(0_u8), |acc, val| acc + val);
            
            let mut p2_result = turn(prev_p2, &mut die);
            p2_result
                .drain_filter(|turn_outcome, &mut outcome_count| {
                    if turn_outcome.score >= TARGET_SCORE.into() {
                        p2_win_count += outcome_count;
                        true
                    } else {

                        false
                    }
                });


            total_p1_win_count += p1_win_count * prev_p2_outcomes;
            total_p2_win_count += p2_win_count * p1_no_win_count;
            
            (p1_result, p2_result)
        });

    println!("{:?} {:?}", total_p1_win_count, total_p2_win_count);
}
