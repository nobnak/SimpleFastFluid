using Unity.Mathematics;

namespace SimpleAndFastFluids {

    public interface IEffect {

        void Next(float dt);
        void Prepare(int2 size);
    }
}