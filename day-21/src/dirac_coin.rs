use crate::die::{Die, TossOutcome};

pub struct DiracCoin {
    rolls: u64
}

const DIRAC_OUTCOMES: [TossOutcome; 3] = [
    TossOutcome { toss_sum: 2, outcome_count: 1 },
    TossOutcome { toss_sum: 3, outcome_count: 2 },
    TossOutcome { toss_sum: 4, outcome_count: 1 },
];

impl Die for DiracCoin {
    fn roll_turn(&mut self) -> &[TossOutcome] {
        self.rolls += 2;
        &DIRAC_OUTCOMES
    }

    fn total_rolls(&self) -> u64 {
        self.rolls
    }
}

impl DiracCoin {
    pub fn new() -> Self {
        DiracCoin { rolls: 0 }
    }
}
