pub struct TossOutcome {
    pub toss_sum: u8,
    pub outcome_count: u8
}

pub trait Die {
    fn roll_turn(&mut self) -> &[TossOutcome];
    fn total_rolls(&self) -> u64;
}
